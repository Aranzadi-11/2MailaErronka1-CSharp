package com.example.osiskitchen.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import java.net.HttpURLConnection
import java.net.URL
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.json.JSONArray
import org.json.JSONObject
import org.json.JSONTokener

data class KitchenOrdersUiState(
    val isLoading: Boolean = false,
    val error: String? = null,
    val groups: List<KitchenOrderGroup> = emptyList(),
    val updatingKomandaIds: Set<Int> = emptySet()
)

data class KitchenOrderGroup(
    val zerbitzuaId: Int,
    val erreserbaId: Int?,
    val customerName: String?,
    val personCount: Int?,
    val tablesLabel: String?,
    val komandak: List<KitchenKomanda>
)

data class KitchenKomanda(
    val id: Int,
    val plateraId: Int,
    val plateraIzena: String,
    val kategoriaId: Int?,
    val kategoriaIzena: String?,
    val kopurua: Int,
    val egoera: Boolean
)

class KitchenOrdersViewModel : ViewModel() {
    private val apiBaseUrlLanPrimary = "http://192.168.10.5:5093/api"

    private val _uiState = MutableStateFlow(KitchenOrdersUiState())
    val uiState: StateFlow<KitchenOrdersUiState> = _uiState

    private data class ZerbitzuaLite(
        val id: Int,
        val mahaiaId: Int?,
        val erreserbaId: Int?
    )

    private data class ErreserbaLite(
        val id: Int,
        val mahaiaId: Int,
        val izena: String?,
        val pertsonaKop: Int?
    )

    private data class MahaiaLite(
        val id: Int,
        val mahaiaZbk: Int
    )

    private data class PlateraLite(
        val id: Int,
        val izena: String,
        val kategoriaId: Int?
    )

    private data class KategoriaLite(
        val id: Int,
        val izena: String
    )

    private data class ZerbitzuXehetasunaLite(
        val id: Int,
        val zerbitzuaId: Int,
        val plateraId: Int,
        val kantitatea: Int,
        val prezioUnitarioa: Double,
        val zerbitzatuta: Boolean
    )

    fun refresh() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val groups = withContext(Dispatchers.IO) { loadGroups() }
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    groups = groups
                )
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = e.message ?: e.javaClass.simpleName
                )
            }
        }
    }

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }

    fun setKomandaEgoera(komandaId: Int, egoera: Boolean) {
        val current = _uiState.value
        if (current.updatingKomandaIds.contains(komandaId)) return

        val updatedGroups =
            current.groups.map { group ->
                group.copy(
                    komandak = group.komandak.map { k ->
                        if (k.id == komandaId) k.copy(egoera = egoera) else k
                    }
                )
            }

        _uiState.value = current.copy(
            groups = updatedGroups,
            updatingKomandaIds = current.updatingKomandaIds + komandaId,
            error = null
        )

        viewModelScope.launch {
            try {
                withContext(Dispatchers.IO) { patchKomandaEgoera(komandaId, egoera) }
                val after = _uiState.value
                _uiState.value = after.copy(
                    updatingKomandaIds = after.updatingKomandaIds - komandaId
                )
            } catch (e: Exception) {
                val after = _uiState.value
                val revertedGroups =
                    after.groups.map { group ->
                        group.copy(
                            komandak = group.komandak.map { k ->
                                if (k.id == komandaId) k.copy(egoera = !egoera) else k
                            }
                        )
                    }

                _uiState.value = after.copy(
                    groups = revertedGroups,
                    updatingKomandaIds = after.updatingKomandaIds - komandaId,
                    error = e.message ?: e.javaClass.simpleName
                )
            }
        }
    }

    private fun loadGroups(): List<KitchenOrderGroup> {
        val zerbitzuak = fetchZerbitzuakGaur()
        val erreserbak = fetchErreserbak().associateBy { it.id }
        val mahaiak = fetchMahaiak().associateBy { it.id }
        val platerak = fetchPlaterak().associateBy { it.id }
        val kategoriak = fetchKategoriak().associateBy { it.id }

        val groups = ArrayList<KitchenOrderGroup>()

        for (zerbitzua in zerbitzuak) {
            val xehetasunak = fetchXehetasunakByZerbitzua(zerbitzua.id)
            if (xehetasunak.isEmpty()) continue

            val erreserba = zerbitzua.erreserbaId?.let { erreserbak[it] }
            val mahaiaId = erreserba?.mahaiaId ?: zerbitzua.mahaiaId
            val tablesLabel = mahaiaId?.let { id ->
                val zenbakia = mahaiak[id]?.mahaiaZbk
                if (zenbakia != null) "$zenbakia. mahaia" else null
            }

            val komandak =
                xehetasunak.mapNotNull { x ->
                    val platera = platerak[x.plateraId] ?: return@mapNotNull null
                    val kategoriaId = platera.kategoriaId
                    val kategoriaIzena = kategoriaId?.let { kategoriak[it]?.izena }

                    KitchenKomanda(
                        id = x.id,
                        plateraId = x.plateraId,
                        plateraIzena = platera.izena,
                        kategoriaId = kategoriaId,
                        kategoriaIzena = kategoriaIzena,
                        kopurua = x.kantitatea,
                        egoera = x.zerbitzatuta
                    )
                }.sortedWith(compareBy<KitchenKomanda> { it.egoera }.thenBy { it.id })

            if (komandak.isEmpty()) continue

            groups.add(
                KitchenOrderGroup(
                    zerbitzuaId = zerbitzua.id,
                    erreserbaId = zerbitzua.erreserbaId,
                    customerName = erreserba?.izena,
                    personCount = erreserba?.pertsonaKop,
                    tablesLabel = tablesLabel,
                    komandak = komandak
                )
            )
        }

        return groups.sortedWith(
            compareBy<KitchenOrderGroup> { it.tablesLabel ?: "" }
                .thenBy { it.zerbitzuaId }
        )
    }

    private fun fetchZerbitzuakGaur(): List<ZerbitzuaLite> {
        val url = "$apiBaseUrlLanPrimary/Zerbitzuak/gaur"
        val array = getArrayFromUrl(url)

        val result = ArrayList<ZerbitzuaLite>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val mahaiaId = obj.optInt("mahaiaId", obj.optInt("MahaiaId", -1)).takeIf { it > 0 }
            val erreserbaId = obj.optInt("erreserbaId", obj.optInt("ErreserbaId", -1)).takeIf { it > 0 }

            result.add(
                ZerbitzuaLite(
                    id = id,
                    mahaiaId = mahaiaId,
                    erreserbaId = erreserbaId
                )
            )
        }

        return result
    }

    private fun fetchErreserbak(): List<ErreserbaLite> {
        val url = "$apiBaseUrlLanPrimary/Erreserbak"
        val array = getArrayFromUrl(url)

        val result = ArrayList<ErreserbaLite>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val mahaiaId = obj.optInt("mahaiaId", obj.optInt("MahaiaId", -1)).takeIf { it > 0 } ?: continue
            val izena = obj.optString("izena", obj.optString("Izena", "")).trim().ifBlank { null }
            val pertsonaKop = obj.optInt("pertsonaKop", obj.optInt("PertsonaKop", -1)).takeIf { it > 0 }

            result.add(
                ErreserbaLite(
                    id = id,
                    mahaiaId = mahaiaId,
                    izena = izena,
                    pertsonaKop = pertsonaKop
                )
            )
        }

        return result
    }

    private fun fetchMahaiak(): List<MahaiaLite> {
        val url = "$apiBaseUrlLanPrimary/Mahaiak"
        val array = getArrayFromUrl(url)

        val result = ArrayList<MahaiaLite>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val mahaiaZbk = obj.optInt("mahaiaZbk", obj.optInt("MahaiaZbk", -1)).takeIf { it > 0 } ?: continue

            result.add(
                MahaiaLite(
                    id = id,
                    mahaiaZbk = mahaiaZbk
                )
            )
        }

        return result
    }

    private fun fetchPlaterak(): List<PlateraLite> {
        val url = "$apiBaseUrlLanPrimary/Platerak"
        val array = getArrayFromUrl(url)

        val result = ArrayList<PlateraLite>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val izena = obj.optString("izena", obj.optString("Izena", "")).trim().ifBlank { "Platera $id" }
            val kategoriaId = obj.optInt("kategoriaId", obj.optInt("KategoriaId", -1)).takeIf { it > 0 }

            result.add(
                PlateraLite(
                    id = id,
                    izena = izena,
                    kategoriaId = kategoriaId
                )
            )
        }

        return result
    }

    private fun fetchKategoriak(): List<KategoriaLite> {
        val url = "$apiBaseUrlLanPrimary/Kategoria"
        val array = getArrayFromUrl(url)

        val result = ArrayList<KategoriaLite>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val izena = obj.optString("izena", obj.optString("Izena", "")).trim().ifBlank { "Kategoria $id" }

            result.add(
                KategoriaLite(
                    id = id,
                    izena = izena
                )
            )
        }

        return result
    }

    private fun fetchXehetasunakByZerbitzua(zerbitzuaId: Int): List<ZerbitzuXehetasunaLite> {
        val url = "$apiBaseUrlLanPrimary/ZerbitzuXehetasunak/zerbitzua/$zerbitzuaId"
        val array = getArrayFromUrl(url)

        val result = ArrayList<ZerbitzuXehetasunaLite>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val plateraId = obj.optInt("plateraId", obj.optInt("PlateraId", -1)).takeIf { it > 0 } ?: continue
            val kantitatea = obj.optInt("kantitatea", obj.optInt("Kantitatea", 0)).takeIf { it > 0 } ?: 0
            val prezioUnitarioa = obj.optDouble("prezioUnitarioa", obj.optDouble("PrezioUnitarioa", 0.0))
            val zerbitzatuta = obj.optBoolean("zerbitzatuta", obj.optBoolean("Zerbitzatuta", false))

            result.add(
                ZerbitzuXehetasunaLite(
                    id = id,
                    zerbitzuaId = zerbitzuaId,
                    plateraId = plateraId,
                    kantitatea = kantitatea,
                    prezioUnitarioa = prezioUnitarioa,
                    zerbitzatuta = zerbitzatuta
                )
            )
        }

        return result
    }

    private fun patchKomandaEgoera(komandaId: Int, egoera: Boolean) {
        // En lugar de usar el endpoint PATCH que activa la lógica de stock en el Repo,
        // usamos el endpoint PUT (Eguneratu) que solo guarda los cambios en la tabla.
        
        // 1. Obtener los datos actuales del detalle
        val urlGet = "$apiBaseUrlLanPrimary/ZerbitzuXehetasunak/$komandaId"
        val (codeGet, bodyGet) = httpGet(urlGet)
        if (codeGet !in 200..299) {
            throw IllegalStateException("Ezin izan da xehetasuna kargatu (code=$codeGet)")
        }
        
        val obj = JSONObject(bodyGet)
        val zerbitzuaId = obj.optInt("zerbitzuaId", obj.optInt("ZerbitzuaId", -1))
        val plateraId = obj.optInt("plateraId", obj.optInt("PlateraId", -1))
        val kantitatea = obj.optInt("kantitatea", obj.optInt("Kantitatea", 0))
        val prezioUnitarioa = obj.optDouble("prezioUnitarioa", obj.optDouble("PrezioUnitarioa", 0.0))

        // 2. Enviar el PUT con el nuevo estado
        val urlPut = "$apiBaseUrlLanPrimary/ZerbitzuXehetasunak/$komandaId"
        val conn =
            (URL(urlPut).openConnection() as HttpURLConnection).apply {
                requestMethod = "PUT"
                setRequestProperty("Content-Type", "application/json; charset=utf-8")
                setRequestProperty("Accept", "application/json")
                doOutput = true
                connectTimeout = 15000
                readTimeout = 15000
            }

        val payload = JSONObject()
            .put("zerbitzuaId", zerbitzuaId)
            .put("plateraId", plateraId)
            .put("kantitatea", kantitatea)
            .put("prezioUnitarioa", prezioUnitarioa)
            .put("zerbitzatuta", egoera)

        conn.outputStream.use { it.write(payload.toString().toByteArray(Charsets.UTF_8)) }

        val code = conn.responseCode
        if (code in 200..299) return

        val body = (conn.errorStream ?: conn.inputStream)?.bufferedReader()?.use { it.readText() }.orEmpty()
        throw IllegalStateException("Ezin izan da egoera eguneratu (url=$urlPut code=$code body=${body.take(200)})")
    }

    private fun getArrayFromUrl(url: String): JSONArray {
        val (code, text) = httpGet(url)
        if (code !in 200..299) {
            throw IllegalStateException("Errorea datuak kargatzean (url=$url code=$code body=${text.take(200)})")
        }

        val root = JSONTokener(text).nextValue()
        return when (root) {
            is JSONArray -> root
            is JSONObject -> {
                root.optJSONArray("data")
                    ?: root.optJSONArray("result")
                    ?: root.optJSONArray("items")
                    ?: root.optJSONArray("Items")
                    ?: root.optJSONArray("\$values")
                    ?: JSONArray()
            }
            else -> JSONArray()
        }
    }

    private fun httpGet(url: String): Pair<Int, String> {
        val conn =
            (URL(url).openConnection() as HttpURLConnection).apply {
                requestMethod = "GET"
                setRequestProperty("Accept", "application/json")
                connectTimeout = 15000
                readTimeout = 15000
            }

        val code = conn.responseCode
        val stream = if (code in 200..299) conn.inputStream else conn.errorStream
        val body = stream?.bufferedReader()?.use { it.readText() }.orEmpty()
        return code to body
    }
}