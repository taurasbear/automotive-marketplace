import { Input } from "@/components/ui/input";
import imageCompression from "browser-image-compression";
import { ControllerRenderProps } from "react-hook-form";
import { CreateListingFormData } from "../types/CreateListingFormData";

type ImageUploadInputProps = {
  field: ControllerRenderProps<CreateListingFormData, "images">;
};

const ImageUploadInput = ({ field }: ImageUploadInputProps) => {
  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const files = Array.from(e.target.files);
      const compressionPromises = files.map(
        async (image) =>
          await imageCompression(image, {
            maxSizeMB: 0.4,
            useWebWorker: true,
            maxWidthOrHeight: 1920,
          }),
      );

      const compressedImages = await Promise.all(compressionPromises);

      field.onChange(compressedImages);
    }
  };

  return (
    <Input type="file" multiple accept="image/*" onChange={handleImageUpload} />
  );
};

export default ImageUploadInput;
