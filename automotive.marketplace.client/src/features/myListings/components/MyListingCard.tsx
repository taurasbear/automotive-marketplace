import { useState } from "react";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import {
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  Heart,
  MessageSquare,
  Pencil,
  Trash2,
} from "lucide-react";
import { IoLocationOutline } from "react-icons/io5";
import { MdOutlineLocalGasStation } from "react-icons/md";
import { PiEngine } from "react-icons/pi";
import { TbManualGearbox } from "react-icons/tb";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import ImageHoverGallery from "@/components/gallery/ImageHoverGallery";
import { ListingCardBadge } from "@/features/listingList";
import type { ConversationSummary } from "@/features/chat";
import { useAppSelector } from "@/hooks/redux";
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";

import { GetMyListingsResponse } from "../types/GetMyListingsResponse";
import { useDeleteMyListing } from "../api/useDeleteMyListing";
import { useUpdateListingStatus } from "../api/useUpdateListingStatus";
import ListingBuyerPanel from "./ListingBuyerPanel";
import { SellerInsightsPanel } from "./SellerInsightsPanel";

type MyListingCardProps = {
  listing: GetMyListingsResponse;
  onStartChat: (conversation: ConversationSummary) => void;
};

function StatusBadge({ status }: { status: string }) {
  const { t } = useTranslation("myListings");
  const isSold = status === "Sold" || status === "Bought";
  const isActive = status === "Active" || status === "Approved" || status === "Available";

  const statusKey = (() => {
    if (isSold) return "card.sold";
    if (isActive) return "card.active";
    if (status === "OnHold") return "card.onHold";
    if (status === "Removed") return "card.removed";
    return "card.active";
  })();

  return (
    <span
      className={`rounded px-2 py-0.5 text-xs font-medium ${
        isSold
          ? "bg-gray-700/80 text-gray-100"
          : isActive
            ? "bg-green-600/90 text-white"
            : "bg-yellow-500/90 text-white"
      }`}
    >
      {t(statusKey)}
    </span>
  );
}

export default function MyListingCard({
  listing,
  onStartChat,
}: MyListingCardProps) {
  const { t } = useTranslation("myListings");
  const { t: tListings } = useTranslation("listings");
  const { userId } = useAppSelector((state) => state.auth);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [markAsSoldDialogOpen, setMarkAsSoldDialogOpen] = useState(false);
  const [panelOpen, setPanelOpen] = useState(false);
  const deleteListingMutation = useDeleteMyListing();
  const updateListingStatusMutation = useUpdateListingStatus();

  const isSold = listing.status === "Sold" || listing.status === "Bought";
  const isAvailable = listing.status === "Available";

  const handleDelete = () => {
    deleteListingMutation.mutate(
      { id: listing.id },
      { onSuccess: () => setDeleteDialogOpen(false) },
    );
  };

  const handleMarkAsSold = () => {
    updateListingStatusMutation.mutate(
      { id: listing.id, status: "Bought" },
      { onSuccess: () => setMarkAsSoldDialogOpen(false) },
    );
  };

  return (
    <div className={isSold ? "opacity-50" : ""}>
      <div className="bg-card border-border grid w-204 gap-8 border-1 md:grid-cols-2">
        {/* Left — Image gallery */}
        <div className="relative flex flex-shrink-0 py-5">
          <ImageHoverGallery
            images={listing.images}
            className="aspect-[4/3] w-full"
          />
          {/* Status badge — top-left overlay */}
          <div className="absolute top-7 left-2">
            <StatusBadge status={listing.status} />
          </div>
          {/* Image count — top-right overlay */}
          {listing.images.length > 1 && (
            <div className="absolute top-7 right-2 rounded bg-black/60 px-1.5 py-0.5 text-xs text-white">
              {listing.images.length}
            </div>
          )}
        </div>

        {/* Right — Details */}
        <div className="flex min-w-0 flex-grow flex-col justify-between gap-2 pt-4 pr-4 pb-2">
          <div className="truncate">
            <p className="truncate font-sans text-xs">
              {listing.isUsed ? t("fields.used") : t("fields.new")}
            </p>
            <p className="font-sans text-xl">
              {listing.year} {listing.makeName} {listing.modelName}
            </p>
            <p className="font-sans text-xs">
              {formatNumber(listing.mileage)} km
            </p>
            <p className="font-sans text-3xl font-bold">
              {formatCurrency(listing.price)} €
            </p>
          </div>

          {/* Spec badges */}
          <div className="grid grid-cols-2 justify-items-stretch gap-x-0 gap-y-4">
            <div className="flex justify-self-start">
              <ListingCardBadge
                Icon={<PiEngine className="h-8 w-8" />}
                title="Variklis"
                stat={`${listing.engineSizeMl / 1000} l ${listing.powerKw} kW`}
              />
            </div>
            <div className="flex justify-self-end">
              <ListingCardBadge
                Icon={<MdOutlineLocalGasStation className="h-8 w-8" />}
                title="Kuras"
                stat={translateVehicleAttr("fuel", listing.fuelName, tListings)}
              />
            </div>
            <div className="flex justify-self-start">
              <ListingCardBadge
                Icon={<TbManualGearbox className="h-8 w-8" />}
                title="Pavarų dėžė"
                stat={translateVehicleAttr("transmission", listing.transmissionName, tListings)}
              />
            </div>
            <div className="flex justify-self-end">
              <ListingCardBadge
                Icon={<IoLocationOutline className="h-8 w-8" />}
                title={t("card.location")}
                stat={listing.municipalityName}
              />
            </div>
          </div>

          {/* Bottom row — defect badge and action buttons */}
          <div className="flex flex-col gap-1">
            {/* Defect badge row */}
            {listing.defectCount > 0 && (
              <div>
                <Badge
                  variant="outline"
                  className="border-amber-200 bg-amber-50 text-amber-700"
                >
                  <AlertTriangle className="mr-1 h-3 w-3" />
                  {t("card.defects", { count: listing.defectCount })}
                </Badge>
              </div>
            )}

            {/* Action buttons row */}
            <div className="flex items-center justify-end gap-2">
              {/* Action buttons — only when not sold */}
              {!isSold && (
                <div className="flex flex-wrap items-center gap-2">
                  {/* Buyer panel toggle */}
                  <button
                    type="button"
                    onClick={() => setPanelOpen((o) => !o)}
                    className="text-muted-foreground hover:text-foreground flex items-center gap-1 text-xs"
                  >
                    <Heart className="h-4 w-4" />
                    <span>{listing.likeCount}</span>
                    <MessageSquare className="h-4 w-4" />
                    <span>{listing.conversationCount}</span>
                    {panelOpen ? (
                      <ChevronUp className="h-4 w-4" />
                    ) : (
                      <ChevronDown className="h-4 w-4" />
                    )}
                  </button>

                  {/* Edit button */}
                  <Button asChild size="sm" variant="outline">
                    <Link to="/my-listings/$id" params={{ id: listing.id }}>
                      <Pencil className="mr-1 h-4 w-4" />
                      {t("card.edit")}
                    </Link>
                  </Button>

                  {/* Mark as Sold button — only when Available */}
                  {isAvailable && (
                    <AlertDialog
                      open={markAsSoldDialogOpen}
                      onOpenChange={setMarkAsSoldDialogOpen}
                    >
                      <AlertDialogTrigger asChild>
                        <Button size="sm" variant="outline">
                          {t("card.markAsSold")}
                        </Button>
                      </AlertDialogTrigger>
                      <AlertDialogContent>
                        <AlertDialogHeader>
                          <AlertDialogTitle>{t("card.markAsSoldConfirmTitle")}</AlertDialogTitle>
                          <AlertDialogDescription>{t("card.markAsSoldConfirmDescription")}</AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                          <AlertDialogCancel>{t("card.cancel")}</AlertDialogCancel>
                          <AlertDialogAction
                            onClick={handleMarkAsSold}
                            disabled={updateListingStatusMutation.isPending}
                          >
                            {updateListingStatusMutation.isPending
                              ? "..."
                              : t("card.confirm")}
                          </AlertDialogAction>
                        </AlertDialogFooter>
                      </AlertDialogContent>
                    </AlertDialog>
                  )}

                  {/* Delete button */}
                  <AlertDialog
                    open={deleteDialogOpen}
                    onOpenChange={setDeleteDialogOpen}
                  >
                    <AlertDialogTrigger asChild>
                      <Button
                        size="sm"
                        variant="outline"
                        className="text-red-600 hover:text-red-700"
                      >
                        <Trash2 className="mr-1 h-4 w-4" />
                        {t("card.delete")}
                      </Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle>
                          {t("detail.deleteConfirmTitle")}
                        </AlertDialogTitle>
                        <AlertDialogDescription>
                          {t("detail.deleteConfirmDescription")}
                        </AlertDialogDescription>
                      </AlertDialogHeader>
                      <AlertDialogFooter>
                        <AlertDialogCancel>
                          {t("detail.cancel")}
                        </AlertDialogCancel>
                        <AlertDialogAction
                          onClick={handleDelete}
                          className="bg-red-600 hover:bg-red-700"
                          disabled={deleteListingMutation.isPending}
                        >
                          {deleteListingMutation.isPending
                            ? "..."
                            : t("detail.confirm")}
                        </AlertDialogAction>
                      </AlertDialogFooter>
                    </AlertDialogContent>
                  </AlertDialog>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Buyer panel — expands below the card */}
      {panelOpen && !isSold && (
        <ListingBuyerPanel
          listingId={listing.id}
          listingTitle={`${listing.year} ${listing.makeName} ${listing.modelName}`}
          listingPrice={listing.price}
          listingMake={listing.makeName}
          listingMileage={listing.mileage}
          listingThumbnail={listing.thumbnail}
          sellerId={userId ?? ""}
          onStartChat={onStartChat}
        />
      )}
      <div className="max-w-204">
        <SellerInsightsPanel listingId={listing.id} />
      </div>
    </div>
  );
}
