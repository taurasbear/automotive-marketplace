import { useState, useEffect } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useTranslation } from "react-i18next";

type GalleryImage = {
  url: string;
  altText: string;
  defectName?: string;
};

type ImageArrowGalleryProps = {
  images: GalleryImage[];
  className?: string;
};

export default function ImageArrowGallery({
  images,
  className = "",
}: ImageArrowGalleryProps) {
  const { t } = useTranslation("common");
  const [activeIndex, setActiveIndex] = useState(0);

  // Handle keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "ArrowLeft") {
        setActiveIndex((prev) => (prev === 0 ? images.length - 1 : prev - 1));
      } else if (e.key === "ArrowRight") {
        setActiveIndex((prev) => (prev === images.length - 1 ? 0 : prev + 1));
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [images.length]);

  // Reset active index if images change
  useEffect(() => {
    if (activeIndex >= images.length) {
      setActiveIndex(0);
    }
  }, [images.length, activeIndex]);

  if (images.length === 0) {
    return (
      <div
        className={`flex aspect-video w-full items-center justify-center rounded-lg bg-gray-200 dark:bg-gray-800 ${className}`}
      >
        <span className="text-lg text-gray-500">
          {t("gallery.noImagesAvailable")}
        </span>
      </div>
    );
  }

  const goToPrevious = () => {
    setActiveIndex((prev) => (prev === 0 ? images.length - 1 : prev - 1));
  };

  const goToNext = () => {
    setActiveIndex((prev) => (prev === images.length - 1 ? 0 : prev + 1));
  };

  const currentImage = images[activeIndex];

  // Find the index where defect images start
  const firstDefectIndex = images.findIndex((img) => img.defectName);

  return (
    <div className={`relative ${className}`}>
      {/* Main image container */}
      <div className="relative">
        <img
          className="aspect-video w-full rounded-t-lg object-cover"
          alt={currentImage.altText}
          src={currentImage.url}
        />

        {/* Counter in top-right */}
        <div className="absolute top-4 right-4 rounded bg-black/50 px-2 py-1 text-sm text-white">
          {activeIndex + 1} / {images.length}
        </div>

        {/* Arrow buttons - only show if more than 1 image */}
        {images.length > 1 && (
          <>
            <button
              onClick={goToPrevious}
              className="absolute top-1/2 left-4 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white transition-colors hover:bg-black/75"
              aria-label={t("gallery.previousImage")}
            >
              <ChevronLeft className="h-6 w-6" />
            </button>
            <button
              onClick={goToNext}
              className="absolute top-1/2 right-4 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white transition-colors hover:bg-black/75"
              aria-label={t("gallery.nextImage")}
            >
              <ChevronRight className="h-6 w-6" />
            </button>
          </>
        )}
      </div>

      {/* Defect info bar */}
      {currentImage.defectName && (
        <div className="border-l-4 border-amber-500 bg-amber-500/10 px-3 py-1.5 text-sm text-amber-700 dark:text-amber-400">
          <strong>{t("gallery.defectLabel")}</strong> {currentImage.defectName}
        </div>
      )}

      {/* Thumbnail strip */}
      {images.length > 1 && (
        <div className="mt-2 flex gap-2 overflow-x-auto pb-2 rounded-b-lg">
          {images.map((image, index) => {
            const isDefectImage = !!image.defectName;
            const isActive = index === activeIndex;

            return (
              <div key={index} className="flex items-center">
                {/* Separator before first defect image */}
                {firstDefectIndex === index && index > 0 && (
                  <div className="bg-border mx-1 w-px self-stretch" />
                )}

                <button
                  onClick={() => setActiveIndex(index)}
                  className={`h-18 w-18 flex-shrink-0 overflow-hidden rounded-lg transition-all ${
                    isActive
                      ? isDefectImage
                        ? "ring-2 ring-amber-500"
                        : "ring-primary ring-2"
                      : isDefectImage
                        ? "ring-1 ring-amber-500/50 hover:ring-2 hover:ring-amber-500"
                        : "hover:ring-primary/50 hover:ring-2"
                  }`}
                >
                  <img
                    className="h-full w-full object-cover"
                    alt={image.altText}
                    src={image.url}
                  />
                </button>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
