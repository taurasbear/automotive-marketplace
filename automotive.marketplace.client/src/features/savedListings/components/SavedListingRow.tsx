import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "@tanstack/react-router";
import { IoHeart } from "react-icons/io5";
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";
import { useToggleLike } from "../api/useToggleLike";
import type { SavedListing } from "../types/SavedListing";
import NoteEditor from "./NoteEditor";

interface SavedListingRowProps {
  listing: SavedListing;
}

const SavedListingRow = ({ listing }: SavedListingRowProps) => {
  const { t } = useTranslation("saved");
  const { t: tListings } = useTranslation("listings");
  const [isHovered, setIsHovered] = useState(false);
  const toggleLike = useToggleLike();

  const handleUnlike = () => {
    toggleLike.mutate({ listingId: listing.listingId });
  };

  return (
    <div
      className="border-border hover:bg-muted/50 flex gap-4 border-b p-4 transition-colors"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      {/* Thumbnail */}
      <div className="h-20 w-28 flex-shrink-0 overflow-hidden rounded">
        {listing.thumbnail ? (
          <img
            src={listing.thumbnail.url}
            alt={listing.thumbnail.altText}
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="bg-muted flex h-full w-full items-center justify-center text-xs">
            {t("row.noImage")}
          </div>
        )}
      </div>

      {/* Content */}
      <div className="flex min-w-0 flex-1 flex-col">
        <div className="flex items-start justify-between">
          <div className="min-w-0">
            <Link
              to="/listing/$id"
              params={{ id: listing.listingId }}
              className="block truncate font-medium text-primary hover:underline"
            >
              {listing.title}
            </Link>
            <p className="text-muted-foreground text-sm">
              {formatCurrency(listing.price)} € · {listing.municipalityName} ·{" "}
              {formatNumber(listing.mileage)} km ·{" "}
              {translateVehicleAttr("fuel", listing.fuelName, tListings)} ·{" "}
              {translateVehicleAttr("transmission", listing.transmissionName, tListings)}
            </p>
          </div>
          <button
            onClick={handleUnlike}
            className="ml-2 flex-shrink-0 text-red-500 transition-opacity hover:opacity-70"
            title={t("row.removeFromSaved")}
          >
            <IoHeart className="h-5 w-5" />
          </button>
        </div>

        <NoteEditor listing={listing} isExpanded={isHovered} />
      </div>
    </div>
  );
};

export default SavedListingRow;
