import { useTranslation } from "react-i18next";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

type Props = {
  listing: GetListingByIdResponse;
};

export function ListingSecondaryDetails({ listing }: Props) {
  const { t } = useTranslation("listings");

  const details = [
    { label: t("details.bodyType"), value: translateVehicleAttr("bodyType", listing.bodyTypeName, t) },
    ...(listing.colour ? [{ label: t("details.colour"), value: listing.colour }] : []),
    { label: t("details.doors"), value: String(listing.doorCount) },
    {
      label: t("details.steering"),
      value: listing.isSteeringWheelRight ? t("details.rightHand") : t("details.leftHand"),
    },
    ...(listing.vin ? [{ label: t("details.vin"), value: listing.vin }] : []),
    { label: t("details.seller"), value: listing.sellerName },
  ];

  return (
    <div className="bg-card text-card-foreground rounded-lg border shadow-sm">
      <div className="p-6">
        <h3 className="text-lg font-semibold">{t("details.additionalDetails")}</h3>
      </div>
      <div className="border-t">
        <dl className="divide-border divide-y">
          {details.map(({ label, value }) => (
            <div key={label} className="grid grid-cols-2 px-6 py-3">
              <dt className="text-muted-foreground text-sm font-medium">{label}</dt>
              <dd className="text-right text-sm">{value}</dd>
            </div>
          ))}
        </dl>
      </div>
    </div>
  );
}