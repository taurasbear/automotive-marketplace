import { useSuspenseQuery, useQuery } from "@tanstack/react-query";
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
import { getDefectCategoriesOptions } from "@/api/defect/getDefectCategoriesOptions";
import { getTranslatedName } from "@/lib/i18n/getTranslatedName";
import { getListingByIdOptions } from "@/features/listingDetails/api/getListingByIdOptions";
import { useUpdateListing } from "@/features/listingDetails/api/useUpdateListing";
import { useDeleteMyListing } from "@/features/myListings/api/useDeleteMyListing";
import EditableField from "./EditableField";
import type { ListingDefectDto } from "@/features/listingDetails/types/GetListingByIdResponse";

type MyListingDetailProps = {
  id: string;
};

const MyListingDetail = ({ id }: MyListingDetailProps) => {
  const { t, i18n } = useTranslation("myListings");
  const navigate = useNavigate();
  const [pendingChanges, setPendingChanges] = useState<
    Record<string, string | number | boolean>
  >({});

  const { data: response } = useSuspenseQuery(getListingByIdOptions({ id }));
  const { data: defectCategories } = useQuery(getDefectCategoriesOptions);
  const listing = response.data;
  const updateMutation = useUpdateListing();
  const deleteMutation = useDeleteMyListing();

  const getDefectDisplayName = (defect: ListingDefectDto): string => {
    if (defect.customName) return defect.customName;
    if (defect.defectCategoryId && defectCategories?.data) {
      const category = defectCategories.data.find((c) => c.id === defect.defectCategoryId);
      if (category) {
        return getTranslatedName(category.translations, i18n.language);
      }
    }
    return defect.defectCategoryName ?? t("common:defects.unknownDefect");
  };

  const galleryImages = [
    ...listing.images.map((img) => ({ url: img.url, altText: img.altText })),
    ...listing.defects.flatMap((defect) =>
      defect.images.map((img) => ({
        url: img.url,
        altText: img.altText,
        defectName: getDefectDisplayName(defect),
      })),
    ),
  ];

  const handleFieldChange = (
    field: string,
    value: string | number | boolean,
  ) => {
    setPendingChanges((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    await updateMutation.mutateAsync({
      id: listing.id,
      price: (pendingChanges.price as number) ?? listing.price,
      mileage: (pendingChanges.mileage as number) ?? listing.mileage,
      city: (pendingChanges.city as string) ?? listing.city,
      description:
        (pendingChanges.description as string) ?? listing.description,
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
    <div className="container mx-auto max-w-4xl space-y-6 px-4 py-6">
      {/* Back link */}
      <Link
        to="/my-listings"
        className="inline-flex items-center gap-2 text-sm hover:underline"
      >
        <ArrowLeft className="h-4 w-4" />
        {t("detail.backToMyListings")}
      </Link>

      {/* Image gallery */}
      {galleryImages.length > 0 && (
        <ImageArrowGallery images={galleryImages} className="w-full rounded-lg" />
      )}

      {/* Header card */}
      <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold">
                {listing.year} {listing.makeName} {listing.modelName}
              </h1>
              <Badge variant={getStatusVariant(listing.status)}>
                {listing.status}
              </Badge>
            </div>
            <p className="text-3xl font-bold mt-3">
              {listing.price.toFixed(0)} €
            </p>
          </div>
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button variant="destructive" size="sm">
                <AlertTriangle className="mr-2 h-4 w-4" />
                {t("detail.deleteListing")}
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
      </div>

      {/* Vehicle Specifications card */}
      <div className="bg-card text-card-foreground rounded-lg border shadow-sm">
        <div className="p-6">
          <h2 className="text-xl font-semibold">{t("vehicleInfo.title")}</h2>
        </div>
        <div className="border-t p-0">
          <dl className="divide-y divide-border">
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.make")}</dt>
              <dd className="text-right text-sm">{listing.makeName}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.model")}</dt>
              <dd className="text-right text-sm">{listing.modelName}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.fuel")}</dt>
              <dd className="text-right text-sm">{listing.fuelName}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.transmission")}</dt>
              <dd className="text-right text-sm">{listing.transmissionName}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.bodyType")}</dt>
              <dd className="text-right text-sm">{listing.bodyTypeName}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.drivetrain")}</dt>
              <dd className="text-right text-sm">{listing.drivetrainName}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.doors")}</dt>
              <dd className="text-right text-sm">{listing.doorCount}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.powerKw")}</dt>
              <dd className="text-right text-sm">{listing.powerKw}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.engineSizeMl")}</dt>
              <dd className="text-right text-sm">{listing.engineSizeMl}</dd>
            </div>
            <div className="grid grid-cols-2 px-6 py-3">
              <dt className="text-sm font-medium text-muted-foreground">{t("vehicleInfo.year")}</dt>
              <dd className="text-right text-sm">{listing.year}</dd>
            </div>
          </dl>
        </div>
      </div>

      {/* Editable Details card */}
      <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
        <h2 className="text-xl font-semibold mb-4">
          {t("vehicleInfo.editableTitle")}
        </h2>
        <div className="divide-y divide-border">
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
            onConfirm={(value) =>
              handleFieldChange("isSteeringWheelRight", value)
            }
          />
        </div>
      </div>

      {/* Defects card */}
      <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
        <h2 className="text-xl font-semibold mb-4">{t("detail.defects")}</h2>
        <DefectSelector
          mode="api"
          listingId={listing.id}
          existingDefects={listing.defects}
        />
      </div>

      {/* Floating save bar */}
      {Object.keys(pendingChanges).length > 0 && (
        <div className="bg-card sticky bottom-0 flex items-center justify-between border-t p-4 shadow-lg">
          <span className="text-muted-foreground text-sm">
            {t("detail.unsavedChanges", {
              count: Object.keys(pendingChanges).length,
            })}
          </span>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => setPendingChanges({})}>
              {t("detail.discard")}
            </Button>
            <Button onClick={handleSave} disabled={updateMutation.isPending}>
              {t("detail.saveChanges")}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

export default MyListingDetail;
