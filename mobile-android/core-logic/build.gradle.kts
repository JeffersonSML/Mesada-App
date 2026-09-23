// Módulo Kotlin puro — sem dependência do Android SDK. Concentra a lógica
// que dá para (e vale a pena) testar fora de um emulador: fila de
// sincronização offline e resolução de conflito
// (docs/adendo-mobile.md#fluxo-offline-first-e-sincronização).
plugins {
    alias(libs.plugins.kotlin.jvm)
}

kotlin {
    jvmToolchain(17)
}

dependencies {
    testImplementation(kotlin("test"))
}

tasks.test {
    useJUnitPlatform()
}
