import { useEffect, useState } from "react";

type ImagePreviewProps = {
  images: Blob[];
  onRemove: (index: number) => void;
};

const ImagePreview = ({ images, onRemove }: ImagePreviewProps) => {
  const [objectUrls, setObjectUrls] = useState<string[]>([]);

  useEffect(() => {
    const urls = images.map((file) => URL.createObjectURL(file));
    setObjectUrls(urls);
    return () => {
      urls.forEach((url) => URL.revokeObjectURL(url));
    };
  }, [images]);

  if (objectUrls.length === 0) return null;

  return (
    <div className="mt-2 flex flex-wrap gap-2">
      {objectUrls.map((url, index) => (
        <div key={index} className="relative h-14 w-14 flex-shrink-0">
          <img
            src={url}
            alt={`Preview ${index + 1}`}
            className="h-14 w-14 rounded object-cover"
          />
          <button
            type="button"
            onClick={() => onRemove(index)}
            className="bg-destructive text-destructive-foreground absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full text-xs leading-none"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
};

export default ImagePreview;
