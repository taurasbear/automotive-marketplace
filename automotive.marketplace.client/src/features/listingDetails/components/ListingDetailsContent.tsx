import { Button } from "@/components/ui/button";
import { PERMISSIONS } from "@/constants/permissions";
import { ChatPanel, useGetOrCreateConversation } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";
import { CompareSearchModal } from "@/features/compareListings";
import { selectUserId } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import { router } from "@/lib/router";
import { formatNumber } from "@/lib/i18n/formatNumber";
import { getTranslatedName } from "@/lib/i18n/getTranslatedName";
import { useSuspenseQuery, useQuery } from "@tanstack/react-query";
import { Trash, Camera } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";
import { getDefectCategoriesOptions } from "@/api/defect/getDefectCategoriesOptions";
import { getListingByIdOptions } from "../api/getListingByIdOptions";
import { useDeleteListing } from "../api/useDeleteListing";
import EditListingDialog from "./EditListingDialog";
import ImageArrowGallery from "@/components/gallery/ImageArrowGallery";
import type { ListingDefectDto } from "@/features/listingDetails/types/GetListingByIdResponse";
import { ScoreCard } from "./ScoreCard";
import { AiSummarySection } from "./AiSummarySection";

type ListingDetailsProps = {
  id: string;
};

const ListingDetailsContent = ({ id }: ListingDetailsProps) => {
  const { t, i18n } = useTranslation("listings");
  const { data: listingQuery } = useSuspenseQuery(
    getListingByIdOptions({ id }),
  );

  const { data: defectCategories } = useQuery(getDefectCategoriesOptions);
  const { mutateAsync: deleteListingAsync } = useDeleteListing();

  const listing = listingQuery.data;
  const permissions = useAppSelector((state) => state.auth.permissions);
  const userId = useAppSelector(selectUserId);

  const canManageListing = permissions.includes(PERMISSIONS.ManageListings);

  const [chatConversation, setChatConversation] =
    useState<ConversationSummary | null>(null);
  const [compareModalOpen, setCompareModalOpen] = useState(false);
  const { mutateAsync: getOrCreateConversation } = useGetOrCreateConversation();

  const isSeller = listing.sellerId === userId;

  const handleContactSeller = async () => {
    const res = await getOrCreateConversation({ listingId: id });
    setChatConversation({
      id: res.data.conversationId,
      listingId: id,
      listingTitle: `${listing.year} ${listing.makeName} ${listing.modelName}`,
      listingThumbnail: listing.images[0] ?? null,
      listingPrice: listing.price,
      listingMake: listing.makeName,
      listingCommercialName: "",
      listingVin: listing.vin ?? null,
      listingMileage: listing.mileage,
      counterpartId: listing.sellerId,
      counterpartUsername: listing.sellerName,
      lastMessage: null,
      lastMessageAt: new Date().toISOString(),
      unreadCount: 0,
      buyerId: userId ?? "",
      sellerId: listing.sellerId,
      buyerHasEngaged: true,
    });
  };

  const getDefectDisplayName = (defect: ListingDefectDto): string => {
    if (defect.customName) return defect.customName;
    if (defect.defectCategoryId && defectCategories?.data) {
      const category = defectCategories.data.find(
        (c) => c.id === defect.defectCategoryId,
      );
      if (category) {
        return getTranslatedName(category.translations, i18n.language);
      }
    }
    return defect.defectCategoryName ?? t("common:defects.unknownDefect");
  };

  const handleDelete = async () => {
    await deleteListingAsync({ id });
    await router.navigate({ to: "/" });
  };

  // Build the gallery images array by combining listing images + defect images
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

  return (
    <>
      <div className="container mx-auto max-w-6xl p-4">
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="space-y-8 lg:col-span-2">
            <ImageArrowGallery
              images={galleryImages}
              className="w-full rounded-lg shadow-lg"
            />
            {listing.description && (
              <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
                <h2 className="mb-4 text-2xl font-semibold">
                  {t("details.description")}
                </h2>
                <p className="text-muted-foreground">{listing.description}</p>
              </div>
            )}
            {listing.defects.length > 0 && (
              <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
                <h2 className="mb-4 text-2xl font-semibold">
                  {t("details.defects")}
                </h2>
                <div className="flex flex-wrap gap-2">
                  {listing.defects.map((defect) => (
                    <span
                      key={defect.id}
                      className="rounded-full border border-amber-500/20 bg-amber-500/10 px-3 py-1 text-sm text-amber-700 dark:text-amber-400"
                    >
                      {getDefectDisplayName(defect)}
                      {defect.images.length > 0 && (
                        <span className="ml-1 inline-flex items-center gap-0.5">
                          ({defect.images.length}{" "}
                          <Camera className="inline h-3.5 w-3.5" />)
                        </span>
                      )}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </div>

          <div className="space-y-6">
            <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <h1 className="text-3xl font-bold">
                    {listing.year} {listing.makeName} {listing.modelName}
                  </h1>
                  <p className="text-primary mt-2 text-3xl font-semibold">
                    {listing.price.toFixed(2)} €
                  </p>
                </div>
                {canManageListing && (
                  <div className="flex flex-shrink-0 gap-2">
                    <EditListingDialog listing={listing} id={id} />
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={handleDelete}
                    >
                      <Trash />
                    </Button>
                  </div>
                )}
              </div>
              <div className="mt-4 flex flex-wrap gap-2">
                <span className="bg-secondary text-secondary-foreground rounded-full border px-3 py-1 text-sm">
                  {listing.isUsed ? t("card.used") : t("card.new")}
                </span>
                <span className="bg-secondary text-secondary-foreground rounded-full border px-3 py-1 text-sm">
                  {listing.municipalityName}
                </span>
              </div>
              {userId && !isSeller && (
                <Button className="mt-4 w-full" onClick={handleContactSeller}>
                  {t("details.contactSeller")}
                </Button>
              )}
              <Button
                variant="outline"
                className="mt-2 w-full"
                onClick={() => setCompareModalOpen(true)}
              >
                {t("details.compareWithAnother")}
              </Button>
            </div>

            <ScoreCard listingId={id} />
            <AiSummarySection listingId={id} />

            <div className="bg-card text-card-foreground rounded-lg border shadow-sm">
              <div className="p-6">
                <h2 className="text-2xl font-semibold">
                  {t("details.specifications")}
                </h2>
              </div>
              <div className="border-t p-0">
                <dl className="divide-border divide-y">
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.mileage")}
                    </dt>
                    <dd className="text-right text-sm">
                      {formatNumber(listing.mileage)} km
                    </dd>
                  </div>
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.engine")}
                    </dt>
                    <dd className="text-right text-sm">
                      {listing.engineSizeMl} ml, {listing.powerKw} kw
                    </dd>
                  </div>
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.transmission")}
                    </dt>
                    <dd className="text-right text-sm">
                      {translateVehicleAttr("transmission", listing.transmissionName, t)}
                    </dd>
                  </div>
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.drivetrain")}
                    </dt>
                    <dd className="text-right text-sm">
                      {translateVehicleAttr("drivetrain", listing.drivetrainName, t)}
                    </dd>
                  </div>
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.fuelType")}
                    </dt>
                    <dd className="text-right text-sm">{translateVehicleAttr("fuel", listing.fuelName, t)}</dd>
                  </div>
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.bodyType")}
                    </dt>
                    <dd className="text-right text-sm">
                      {translateVehicleAttr("bodyType", listing.bodyTypeName, t)}
                    </dd>
                  </div>
                  {listing.colour && (
                    <div className="grid grid-cols-2 px-6 py-3">
                      <dt className="text-muted-foreground text-sm font-medium">
                        {t("details.colour")}
                      </dt>
                      <dd className="text-right text-sm">{listing.colour}</dd>
                    </div>
                  )}
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.doors")}
                    </dt>
                    <dd className="text-right text-sm">{listing.doorCount}</dd>
                  </div>
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.steering")}
                    </dt>
                    <dd className="text-right text-sm">
                      {listing.isSteeringWheelRight
                        ? t("details.rightHand")
                        : t("details.leftHand")}
                    </dd>
                  </div>
                  {listing.vin && (
                    <div className="grid grid-cols-2 px-6 py-3">
                      <dt className="text-muted-foreground text-sm font-medium">
                        {t("details.vin")}
                      </dt>
                      <dd className="text-right text-sm break-all">
                        {listing.vin}
                      </dd>
                    </div>
                  )}
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      {t("details.seller")}
                    </dt>
                    <dd className="text-right text-sm font-semibold">
                      {listing.sellerName}
                    </dd>
                  </div>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>
      {chatConversation && (
        <ChatPanel
          conversation={chatConversation}
          onClose={() => setChatConversation(null)}
        />
      )}
      <CompareSearchModal
        open={compareModalOpen}
        onClose={() => setCompareModalOpen(false)}
        excludeIds={[id]}
        onSelect={(selectedId) => {
          setCompareModalOpen(false);
          void router.navigate({
            to: "/compare",
            search: { a: id, b: selectedId },
          });
        }}
      />
    </>
  );
};

export default ListingDetailsContent;
