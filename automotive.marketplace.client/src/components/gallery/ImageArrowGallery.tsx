import { useState, useEffect } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";

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
      <div className={`aspect-video w-full rounded-lg bg-gray-200 dark:bg-gray-800 flex items-center justify-center ${className}`}>
        <span className="text-gray-500 text-lg">No images available</span>
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
  const firstDefectIndex = images.findIndex(img => img.defectName);

  return (
    <div className={`relative ${className}`}>
      {/* Main image container */}
      <div className="relative">
        <img
          className="aspect-video w-full object-cover rounded-lg"
          alt={currentImage.altText}
          src={currentImage.url}
        />
        
        {/* Counter in top-right */}
        <div className="absolute top-4 right-4 bg-black/50 text-white rounded px-2 py-1 text-sm">
          {activeIndex + 1} / {images.length}
        </div>

        {/* Arrow buttons - only show if more than 1 image */}
        {images.length > 1 && (
          <>
            <button
              onClick={goToPrevious}
              className="absolute left-4 top-1/2 -translate-y-1/2 bg-black/50 text-white rounded-full p-2 hover:bg-black/75 transition-colors"
              aria-label="Previous image"
            >
              <ChevronLeft className="h-6 w-6" />
            </button>
            <button
              onClick={goToNext}
              className="absolute right-4 top-1/2 -translate-y-1/2 bg-black/50 text-white rounded-full p-2 hover:bg-black/75 transition-colors"
              aria-label="Next image"
            >
              <ChevronRight className="h-6 w-6" />
            </button>
          </>
        )}
      </div>

      {/* Defect info bar */}
      {currentImage.defectName && (
        <div className="bg-amber-500/10 text-amber-700 dark:text-amber-400 px-3 py-1.5 text-sm border-l-4 border-amber-500">
          <strong>Defect:</strong> {currentImage.defectName}
        </div>
      )}

      {/* Thumbnail strip */}
      {images.length > 1 && (
        <div className="mt-4 flex overflow-x-auto gap-2 pb-2">
          {images.map((image, index) => {
            const isDefectImage = !!image.defectName;
            const isActive = index === activeIndex;
            
            return (
              <div key={index} className="flex items-center">
                {/* Separator before first defect image */}
                {firstDefectIndex === index && index > 0 && (
                  <div className="mx-1 w-px bg-border self-stretch" />
                )}
                
                <button
                  onClick={() => setActiveIndex(index)}
                  className={`flex-shrink-0 w-18 h-18 rounded-lg overflow-hidden transition-all ${
                    isActive 
                      ? isDefectImage
                        ? "ring-2 ring-amber-500"
                        : "ring-2 ring-primary"
                      : isDefectImage
                        ? "ring-1 ring-amber-500/50 hover:ring-2 hover:ring-amber-500"
                        : "hover:ring-2 hover:ring-primary/50"
                  }`}
                >
                  <img
                    className="w-full h-full object-cover"
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