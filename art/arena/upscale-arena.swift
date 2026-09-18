import Foundation
import CoreImage
import ImageIO
let input = URL(fileURLWithPath:CommandLine.arguments[1])
let output = URL(fileURLWithPath:CommandLine.arguments[2])
let image = CIImage(contentsOf:input)!
let filter = CIFilter(name:"CILanczosScaleTransform")!
filter.setValue(image,forKey:kCIInputImageKey)
filter.setValue(2.5,forKey:kCIInputScaleKey)
filter.setValue(1.0,forKey:kCIInputAspectRatioKey)
let context = CIContext()
try context.writePNGRepresentation(of:filter.outputImage!,to:output,format:.RGBA8,colorSpace:CGColorSpace(name:CGColorSpace.sRGB)!,options:[:])
print(filter.outputImage!.extent)
