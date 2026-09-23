import SwiftUI
import UIKit

/// Wrapper mínimo de UIImagePickerController para captura de foto — SwiftUI
/// não tem um componente nativo de câmera; PhotosPicker (iOS 16+) é só para
/// a biblioteca de fotos, não para capturar uma nova.
struct CameraPicker: UIViewControllerRepresentable {
    var aoCapturar: (UIImage) -> Void

    func makeUIViewController(context: Context) -> UIImagePickerController {
        let controlador = UIImagePickerController()
        controlador.sourceType = .camera
        controlador.delegate = context.coordinator
        return controlador
    }

    func updateUIViewController(_ uiViewController: UIImagePickerController, context: Context) {}

    func makeCoordinator() -> Coordinator {
        Coordinator(aoCapturar: aoCapturar)
    }

    final class Coordinator: NSObject, UIImagePickerControllerDelegate, UINavigationControllerDelegate {
        let aoCapturar: (UIImage) -> Void

        init(aoCapturar: @escaping (UIImage) -> Void) {
            self.aoCapturar = aoCapturar
        }

        func imagePickerController(_ picker: UIImagePickerController, didFinishPickingMediaWithInfo info: [UIImagePickerController.InfoKey: Any]) {
            if let imagem = info[.originalImage] as? UIImage {
                aoCapturar(imagem)
            }
            picker.dismiss(animated: true)
        }

        func imagePickerControllerDidCancel(_ picker: UIImagePickerController) {
            picker.dismiss(animated: true)
        }
    }
}
