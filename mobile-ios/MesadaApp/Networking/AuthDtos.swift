import Foundation

/// Espelha backend/src/Mesada.Api/Contracts/AuthContracts.cs.
struct ResgatarConviteRequest: Encodable {
    let dispositivoId: String
}

struct TokenResponse: Decodable {
    let token: String
}
