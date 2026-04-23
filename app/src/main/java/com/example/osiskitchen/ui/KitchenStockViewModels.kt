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

data class KitchenPlatosStockUiState(
    val isLoading: Boolean = false,
    val error: String? = null,
    val platos: List<KitchenPlatoStock> = emptyList()
)

data class KitchenPlatoStock(
    val id: Int,
    val izena: String,
    val kategoriaId: Int?,
    val kategoriaIzena: String?,
    val erabilgarri: String,
    val prestatuDaitezkeenUnitateak: Int
)

class KitchenPlatosStockViewModel : ViewModel() {
    private val apiBaseUrlLanPrimary = "http://192.168.10.5:5093/api"

    private val _uiState = MutableStateFlow(KitchenPlatosStockUiState())
    val uiState: StateFlow<KitchenPlatosStockUiState> = _uiState

    fun refresh() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val platos = withContext(Dispatchers.IO) { fetchPlatos() }
                _uiState.value = _uiState.value.copy(isLoading = false, platos = platos)
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

    private fun fetchPlatos(): List<KitchenPlatoStock> {
        val candidateUrl = "$apiBaseUrlLanPrimary/Platerak/disponibilitatea"
        val (code, text) = httpGet(candidateUrl)

        if (code !in 200..299) {
            throw IllegalStateException("Ezin izan dira platerak kargatu (url=$candidateUrl code=$code body=${text.take(200)})")
        }

        val root = JSONTokener(text).nextValue()
        val array =
            when (root) {
                is JSONArray -> root
                is JSONObject -> root.optJSONArray("data") ?: root.optJSONArray("result") ?: JSONArray()
                else -> JSONArray()
            }

        val result = ArrayList<KitchenPlatoStock>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val izena = obj.optString("izena", obj.optString("Izena", "")).trim().ifBlank { "Platera $id" }
            val kategoriaId = obj.optInt("kategoriaId", obj.optInt("KategoriaId", -1)).takeIf { it > 0 }
            val kategoriaIzena =
                obj.optString("kategoriaIzena", obj.optString("KategoriaIzena", "")).trim().ifBlank { null }
            val erabilgarri =
                obj.optString("erabilgarri", obj.optString("Erabilgarri", "Bai")).trim().ifBlank { "Bai" }
            val prestatuDaitezkeenUnitateak =
                obj.optInt("prestatuDaitezkeenUnitateak", obj.optInt("PrestatuDaitezkeenUnitateak", 0))

            result.add(
                KitchenPlatoStock(
                    id = id,
                    izena = izena,
                    kategoriaId = kategoriaId,
                    kategoriaIzena = kategoriaIzena,
                    erabilgarri = erabilgarri,
                    prestatuDaitezkeenUnitateak = prestatuDaitezkeenUnitateak
                )
            )
        }

        return result.sortedWith(compareBy<KitchenPlatoStock> { it.kategoriaIzena ?: "" }.thenBy { it.izena })
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

data class KitchenIngredientesStockUiState(
    val isLoading: Boolean = false,
    val error: String? = null,
    val ingredientes: List<KitchenIngredienteStock> = emptyList(),
    val updatingIds: Set<Int> = emptySet()
)

data class KitchenIngredienteStock(
    val id: Int,
    val izena: String,
    val stock: Int,
    val gutxienekoStock: Int,
    val neurriaUnitatea: String?
)

class KitchenIngredientesStockViewModel : ViewModel() {
    private val apiBaseUrlLanPrimary = "http://192.168.10.5:5093/api"

    private val _uiState = MutableStateFlow(KitchenIngredientesStockUiState())
    val uiState: StateFlow<KitchenIngredientesStockUiState> = _uiState

    fun refresh() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val items = withContext(Dispatchers.IO) { fetchIngredientes() }
                _uiState.value = _uiState.value.copy(isLoading = false, ingredientes = items)
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

    fun adjustStock(ingredienteId: Int, delta: Int) {
        if (delta == 0) return
        val current = _uiState.value
        if (current.updatingIds.contains(ingredienteId)) return

        val existing = current.ingredientes.firstOrNull { it.id == ingredienteId } ?: return
        val newStock = existing.stock + delta
        if (newStock < 0) return

        val optimistic =
            current.ingredientes.map {
                if (it.id == ingredienteId) it.copy(stock = newStock) else it
            }

        _uiState.value = current.copy(
            ingredientes = optimistic,
            updatingIds = current.updatingIds + ingredienteId,
            error = null
        )

        viewModelScope.launch {
            try {
                withContext(Dispatchers.IO) { patchIngredienteStock(ingredienteId, delta) }
                val after = _uiState.value
                _uiState.value = after.copy(updatingIds = after.updatingIds - ingredienteId)
            } catch (e: Exception) {
                val after = _uiState.value
                val reverted =
                    after.ingredientes.map {
                        if (it.id == ingredienteId) it.copy(stock = existing.stock) else it
                    }

                _uiState.value = after.copy(
                    ingredientes = reverted,
                    updatingIds = after.updatingIds - ingredienteId,
                    error = e.message ?: e.javaClass.simpleName
                )
            }
        }
    }

    private fun fetchIngredientes(): List<KitchenIngredienteStock> {
        val candidateUrl = "$apiBaseUrlLanPrimary/Inbentarioa"
        val (code, text) = httpGet(candidateUrl)

        if (code !in 200..299) {
            throw IllegalStateException("Ezin izan dira osagaiak kargatu (url=$candidateUrl code=$code body=${text.take(200)})")
        }

        val root = JSONTokener(text).nextValue()
        val array =
            when (root) {
                is JSONArray -> root
                is JSONObject -> root.optJSONArray("data") ?: root.optJSONArray("result") ?: JSONArray()
                else -> JSONArray()
            }

        val result = ArrayList<KitchenIngredienteStock>(array.length())
        for (i in 0 until array.length()) {
            val obj = array.optJSONObject(i) ?: continue
            val id = obj.optInt("id", obj.optInt("Id", -1)).takeIf { it > 0 } ?: continue
            val izena = obj.optString("izena", obj.optString("Izena", "")).trim().ifBlank { "Osagaia $id" }
            val stock = obj.optInt("kantitatea", obj.optInt("Kantitatea", 0))
            val gutxienekoStock = obj.optInt("stockMinimoa", obj.optInt("StockMinimoa", 0))
            val neurriaUnitatea =
                obj.optString("neurriaUnitatea", obj.optString("NeurriaUnitatea", "")).trim().ifBlank { null }

            result.add(
                KitchenIngredienteStock(
                    id = id,
                    izena = izena,
                    stock = stock,
                    gutxienekoStock = gutxienekoStock,
                    neurriaUnitatea = neurriaUnitatea
                )
            )
        }

        return result.sortedWith(
            compareBy<KitchenIngredienteStock> { it.stock > it.gutxienekoStock }
                .thenBy { it.izena }
        )
    }

    private fun patchIngredienteStock(ingredienteId: Int, delta: Int) {
        val candidateUrl = "$apiBaseUrlLanPrimary/Inbentarioa/$ingredienteId/kantitatea"

        val url = URL(candidateUrl)
        val conn =
            (url.openConnection() as HttpURLConnection).apply {
                requestMethod = "PATCH"
                setRequestProperty("Content-Type", "application/json; charset=utf-8")
                setRequestProperty("Accept", "application/json")
                doOutput = true
                connectTimeout = 15000
                readTimeout = 15000
            }

        val jsonBody = """{"aldaketa":$delta}"""
        conn.outputStream.use { it.write(jsonBody.toByteArray(Charsets.UTF_8)) }

        val code = conn.responseCode
        if (code in 200..299) return

        val body = (conn.errorStream ?: conn.inputStream)?.bufferedReader()?.use { it.readText() }.orEmpty()
        throw IllegalStateException("Ezin izan da stock-a eguneratu (url=$candidateUrl code=$code body=${body.take(200)})")
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