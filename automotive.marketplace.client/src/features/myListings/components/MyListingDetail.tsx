import { useSuspenseQuery } from "@tanstack/react-query";
import { Link, useNavigate } from "@tanstack/react-router";
import { ArrowLeft, AlertTriangle } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
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
import ImageArrowGallery from "@/components/gallery/ImageArrowGallery";
import DefectSelector from "@/components/forms/DefectSelector";
import { getListingByIdOptions } from "@/features/listingDetails/api/getListingByIdOptions";
import { useUpdateListing } from "@/features/listingDetails/api/useUpdateListing";
import { useDeleteMyListing } from "@/features/myListings/api/useDeleteMyListing";
import EditableField from "./EditableField";

type MyListingDetailProps = {
  id: string;
};

const MyListingDetail = ({ id }: MyListingDetailProps) => {
  const { t } = useTranslation("myListings");
  const navigate = useNavigate();
  const [pendingChanges, setPendingChanges] = useState<
    Record<string, string | number | boolean>
  >({});

  const { data: response } = useSuspenseQuery(getListingByIdOptions({ id }));
  const listing = response.data;
  const updateMutation = useUpdateListing();
  const deleteMutation = useDeleteMyListing();

  const galleryImages = [
    ...listing.images.map((img) => ({ url: img.url, altText: img.altText })),
    ...listing.defects.flatMap((defect) =>
      defect.images.map((img) => ({
        url: img.url,
        altText: img.altText,
        defectName: defect.customName ?? defect.defectCategoryName ?? "Defect",
      }))
    ),
  ];

  const handleFieldChange = (field: string, value: string | number | boolean) => {
    setPendingChanges((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    await updateMutation.mutateAsync({
      id: listing.id,
      price: (pendingChanges.price as number) ?? listing.price,
      mileage: (pendingChanges.mileage as number) ?? listing.mileage,
      city: (pendingChanges.city as string) ?? listing.city,
      description: (pendingChanges.description as string) ?? listing.description,
      colour: (pendingChanges.colour as string) ?? listing.colour,
      vin: (pendingChanges.vin as string) ?? listing.vin,
      isUsed: (pendingChanges.isUsed as boolean) ?? listing.isUsed,
      isSteeringWheelRight:
        (pendingChanges.isSteeringWheelRight as boolean) ??
        listing.isSteeringWheelRight,
      powerKw: listing.powerKw,
      engineSizeMl: listing.engineSizeMl,
      year: listing.year,
    });
    setPendingChanges({});
  };

  const handleDelete = async () => {
    await deleteMutation.mutateAsync({ id: listing.id });
    await navigate({ to: "/my-listings" });
  };

  const getStatusVariant = (status: string) => {
    switch (status.toLowerCase()) {
      case "active":
        return "default";
      case "sold":
        return "secondary";
      default:
        return "outline";
    }
  };

  return (
    <div className="container mx-auto px-4 py-6 space-y-6">
      {/* Back link */}
      <Link to="/my-listings" className="inline-flex items-center gap-2 text-sm hover:underline">
        <ArrowLeft className="h-4 w-4" />
        {t("detail.backToMyListings")}
      </Link>

      {/* Image gallery */}
      {galleryImages.length > 0 && (
        <div className="aspect-video max-h-96">
          <ImageArrowGallery images={galleryImages} />
        </div>
      )}

      {/* Title area */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold">
            {listing.year} {listing.makeName} {listing.modelName}
          </h1>
          <Badge variant={getStatusVariant(listing.status)} className="mt-2">
            {listing.status}
          </Badge>
        </div>
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button variant="destructive" size="sm">
              <AlertTriangle className="h-4 w-4 mr-2" />
              {t("detail.deleteListing")}
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
                className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              >
                {t("detail.confirm")}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Non-editable info */}
        <div className="space-y-4">
          <h3 className="text-lg font-semibold">Vehicle Information</h3>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Make:</span>
              <span>{listing.makeName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Model:</span>
              <span>{listing.modelName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Fuel:</span>
              <span>{listing.fuelName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Transmission:</span>
              <span>{listing.transmissionName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Body Type:</span>
              <span>{listing.bodyTypeName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Drivetrain:</span>
              <span>{listing.drivetrainName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Doors:</span>
              <span>{listing.doorCount}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Power (kW):</span>
              <span>{listing.powerKw}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Engine Size (ml):</span>
              <span>{listing.engineSizeMl}</span>
            </div>
          </div>
        </div>

        {/* Editable fields */}
        <div className="space-y-4">
          <h3 className="text-lg font-semibold">Editable Information</h3>
          <div className="space-y-1 border rounded-lg p-4">
            <EditableField
              label={t("fields.price")}
              value={listing.price}
              type="number"
              onConfirm={(value) => handleFieldChange("price", value)}
            />
            <EditableField
              label={t("fields.city")}
              value={listing.city}
              type="text"
              onConfirm={(value) => handleFieldChange("city", value)}
            />
            <EditableField
              label={t("fields.mileage")}
              value={listing.mileage}
              type="number"
              onConfirm={(value) => handleFieldChange("mileage", value)}
            />
            <EditableField
              label={t("fields.description")}
              value={listing.description || ""}
              type="textarea"
              onConfirm={(value) => handleFieldChange("description", value)}
            />
            <EditableField
              label={t("fields.colour")}
              value={listing.colour || ""}
              type="text"
              onConfirm={(value) => handleFieldChange("colour", value)}
            />
            <EditableField
              label={t("fields.vin")}
              value={listing.vin || ""}
              type="text"
              onConfirm={(value) => handleFieldChange("vin", value)}
            />
            <EditableField
              label={t("fields.isUsed")}
              value={listing.isUsed}
              type="toggle"
              toggleLabels={{ on: t("fields.used"), off: t("fields.new") }}
              onConfirm={(value) => handleFieldChange("isUsed", value)}
            />
            <EditableField
              label={t("fields.steeringWheel")}
              value={listing.isSteeringWheelRight}
              type="toggle"
              toggleLabels={{ on: t("fields.right"), off: t("fields.left") }}
              onConfirm={(value) => handleFieldChange("isSteeringWheelRight", value)}
            />
          </div>
        </div>
      </div>

      {/* Defect Selector */}
      <div className="space-y-4">
        <h3 className="text-lg font-semibold">{t("detail.defects")}</h3>
        <DefectSelector
          mode="api"
          listingId={listing.id}
          existingDefects={listing.defects}
        />
      </div>

      {/* Floating save bar */}
      {Object.keys(pendingChanges).length > 0 && (
        <div className="sticky bottom-0 bg-card border-t p-4 flex items-center justify-between shadow-lg">
          <span className="text-sm text-muted-foreground">
            {t("detail.unsavedChanges", { count: Object.keys(pendingChanges).length })}
          </span>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => setPendingChanges({})}>
              {t("detail.discard")}
            </Button>
            <Button 
              onClick={handleSave} 
              disabled={updateMutation.isPending}
            >
              {t("detail.saveChanges")}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

export default MyListingDetail;