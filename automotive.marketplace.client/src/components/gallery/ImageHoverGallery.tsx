import { useState } from "react";
import { useTranslation } from "react-i18next";

type ImageHoverGalleryProps = {
  images: { url: string; altText: string }[];
  fallbackUrl?: string;
  className?: string;
};

export default function ImageHoverGallery({
  images,
  fallbackUrl = "https://placehold.co/800x600?text=No+Image",
  className = "",
}: ImageHoverGalleryProps) {
  const { t } = useTranslation("common");
  const [activeIndex, setActiveIndex] = useState(0);

  const displayImages =
    images && images.length > 0
      ? images
      : [{ url: fallbackUrl, altText: t("gallery.noImageAvailable") }];

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (displayImages.length <= 1) return;

    const rect = e.currentTarget.getBoundingClientRect();
    const relativeX = e.clientX - rect.left;
    const zoneIndex = Math.floor(
      (relativeX / rect.width) * displayImages.length,
    );
    const clampedIndex = Math.max(
      0,
      Math.min(zoneIndex, displayImages.length - 1),
    );
    setActiveIndex(clampedIndex);
  };

  const handleMouseLeave = () => {
    setActiveIndex(0);
  };

  return (
    <div
      className={`group bg-muted relative overflow-hidden ${className}`}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
    >
      <img
        className="absolute inset-0 h-full w-full object-cover"
        alt={displayImages[activeIndex].altText}
        src={displayImages[activeIndex].url}
      />

      {/* Dot indicators - only show if more than 1 image and on hover */}
      {displayImages.length > 1 && (
        <div className="absolute bottom-2 left-1/2 flex -translate-x-1/2 space-x-1 opacity-0 transition-opacity group-hover:opacity-100">
          {displayImages.map((_, index) => (
            <div
              key={index}
              className={`h-2 w-2 rounded-full transition-opacity ${
                index === activeIndex ? "bg-white" : "bg-white/50"
              }`}
            />
          ))}
        </div>
      )}
    </div>
  );
}
