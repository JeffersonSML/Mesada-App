// swift-tools-version: 5.9
import PackageDescription

/// Lógica pura (sem UIKit/SwiftUI), espelhando 1:1
/// mobile-android/core-logic — a mesma fila de sincronização offline e
/// resolução de conflito, na mesma linguagem de domínio, para as duas
/// plataformas nativas se comportarem de forma idêntica.
let package = Package(
    name: "MesadaCore",
    platforms: [.iOS(.v16)],
    products: [
        .library(name: "MesadaCore", targets: ["MesadaCore"]),
    ],
    targets: [
        .target(name: "MesadaCore"),
        .testTarget(name: "MesadaCoreTests", dependencies: ["MesadaCore"]),
    ]
)
