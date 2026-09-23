package com.mesadaapp.mobile.data.remote.dto

import kotlinx.serialization.Serializable

/** Espelha backend/src/Mesada.Api/Contracts/AuthContracts.cs — ResgatarConviteRequest. */
@Serializable
data class ResgatarConviteRequest(val dispositivoId: String)

/** Espelha backend/src/Mesada.Api/Contracts/AuthContracts.cs — TokenResponse. */
@Serializable
data class TokenResponse(val token: String)
