import { ListingCardBadge } from "@/features/listingList";
import { PiEngine } from "react-icons/pi";
import { MdOutlineLocalGasStation } from "react-icons/md";
import { TbManualGearbox } from "react-icons/tb";
import { IoSpeedometerOutline } from "react-icons/io5";
import { Calendar, Cog } from "lucide-react";
import { useTranslation } from "react-i18next";
import { formatNumber } from "@/lib/i18n/formatNumber";
import { translateVehicleAttr } from "@/features/listingList/utils/translateVehicleAttr";
import type { GetListingByIdResponse } from "../types/GetListingByIdResponse";

type Props = {
  listing: GetListingByIdResponse;
};

export function ListingKeySpecs({ listing }: Props) {
  const { t } = useTranslation("listings");

  return (
    <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
      <h3 className="mb-4 text-lg font-semibold">{t("details.keySpecs")}</h3>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
        <ListingCardBadge
          Icon={<PiEngine className="h-5 w-5" />}
          title={t("card.engine")}
          stat={`${listing.engineSizeMl / 1000} l ${listing.powerKw} kW`}
        />
        <ListingCardBadge
          Icon={<MdOutlineLocalGasStation className="h-5 w-5" />}
          title={t("card.fuelType")}
          stat={translateVehicleAttr("fuel", listing.fuelName, t)}
        />
        <ListingCardBadge
          Icon={<TbManualGearbox className="h-5 w-5" />}
          title={t("card.gearBox")}
          stat={translateVehicleAttr("transmission", listing.transmissionName, t)}
        />
        <ListingCardBadge
          Icon={<IoSpeedometerOutline className="h-5 w-5" />}
          title={t("details.mileage")}
          stat={`${formatNumber(listing.mileage)} km`}
        />
        <ListingCardBadge
          Icon={<Calendar className="h-5 w-5" />}
          title={t("details.year")}
          stat={String(listing.year)}
        />
        <ListingCardBadge
          Icon={<Cog className="h-5 w-5" />}
          title={t("details.drivetrain")}
          stat={translateVehicleAttr("drivetrain", listing.drivetrainName, t)}
        />
      </div>
    </div>
  );
}