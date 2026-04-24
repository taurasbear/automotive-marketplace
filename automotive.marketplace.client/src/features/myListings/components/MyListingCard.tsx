import { useState } from "react";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { Pencil, Trash2, Camera } from "lucide-react";

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
import { Card } from "@/components/ui/card";

import { GetMyListingsResponse } from "../types/GetMyListingsResponse";
import { useDeleteMyListing } from "../api/useDeleteMyListing";

type MyListingCardProps = {
  listing: GetMyListingsResponse;
};

export default function MyListingCard({ listing }: MyListingCardProps) {
  const { t } = useTranslation("myListings");
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const deleteListingMutation = useDeleteMyListing();

  const isSold = listing.status === "Sold";
  const isActive = listing.status === "Active" || listing.status === "Approved";

  const handleDelete = () => {
    deleteListingMutation.mutate(
      { id: listing.id },
      {
        onSuccess: () => {
          setDeleteDialogOpen(false);
        },
      }
    );
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "EUR",
      minimumFractionDigits: 0,
    }).format(price);
  };

  const formatMileage = (mileage: number) => {
    return new Intl.NumberFormat("en-US").format(mileage);
  };

  return (
    <Card className={`p-4 transition-opacity ${isSold ? "opacity-60" : ""}`}>
      <div className="flex gap-4">
        {/* Thumbnail */}
        <div className="relative shrink-0 w-32 h-24 bg-gray-100 rounded-lg overflow-hidden">
          {listing.thumbnail ? (
            <img
              src={listing.thumbnail.url}
              alt={listing.thumbnail.altText}
              className="w-full h-full object-cover"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center bg-gray-200">
              <Camera className="w-6 h-6 text-gray-400" />
            </div>
          )}
          
          {/* Image count badge */}
          {listing.imageCount > 0 && (
            <div className="absolute bottom-1 right-1 bg-black/70 text-white text-xs px-1.5 py-0.5 rounded">
              {listing.imageCount}
            </div>
          )}
        </div>

        {/* Content */}
        <div className="flex-1 flex flex-col justify-between">
          <div>
            {/* Title */}
            <h3 className="font-semibold text-lg text-gray-900 mb-1">
              {listing.year} {listing.makeName} {listing.modelName}
            </h3>

            {/* Details */}
            <p className="text-gray-600 text-sm mb-2">
              {listing.city} · {formatMileage(listing.mileage)} km · {formatPrice(listing.price)}
            </p>

            {/* Badges */}
            <div className="flex gap-2 flex-wrap">
              <Badge
                variant={isActive ? "default" : "secondary"}
                className={isActive ? "bg-green-100 text-green-800" : ""}
              >
                {isActive ? t("card.active") : t("card.sold")}
              </Badge>

              {listing.defectCount > 0 && (
                <Badge variant="outline" className="bg-amber-50 text-amber-700 border-amber-200">
                  {t("card.defects", { count: listing.defectCount })}
                </Badge>
              )}
            </div>
          </div>

          {/* Actions */}
          {!isSold && (
            <div className="flex gap-2 mt-3">
              <Button asChild size="sm" variant="outline">
                <Link to="/my-listings/$id" params={{ id: listing.id }}>
                  <Pencil className="w-4 h-4 mr-1" />
                  {t("card.edit")}
                </Link>
              </Button>

              <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
                <AlertDialogTrigger asChild>
                  <Button size="sm" variant="outline" className="text-red-600 hover:text-red-700">
                    <Trash2 className="w-4 h-4 mr-1" />
                    {t("card.delete")}
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>{t("detail.deleteConfirmTitle")}</AlertDialogTitle>
                    <AlertDialogDescription>
                      {t("detail.deleteConfirmDescription")}
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>{t("detail.cancel")}</AlertDialogCancel>
                    <AlertDialogAction
                      onClick={handleDelete}
                      className="bg-red-600 hover:bg-red-700"
                      disabled={deleteListingMutation.isPending}
                    >
                      {deleteListingMutation.isPending ? "..." : t("detail.confirm")}
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </div>
          )}
        </div>
      </div>
    </Card>
  );
}