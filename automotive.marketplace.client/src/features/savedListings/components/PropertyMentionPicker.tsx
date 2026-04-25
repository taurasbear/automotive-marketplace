import { useTranslation } from "react-i18next";
import { formatCurrency, formatNumber } from "@/lib/i18n/formatNumber";
import type { SavedListing } from "../types/SavedListing";

interface PropertyMentionPickerProps {
  listing: SavedListing;
  onSelect: (chip: string) => void;
  onClose: () => void;
}

const PROPERTY_FIELDS: {
  key: keyof SavedListing;
  labelKey: string;
  format: (value: unknown) => string;
}[] = [
  {
    key: "mileage",
    labelKey: "propertyMention.mileage",
    format: (v) => `${formatNumber(v as number)} km`,
  },
  {
    key: "price",
    labelKey: "propertyMention.price",
    format: (v) => `${formatCurrency(v as number)} €`,
  },
  {
    key: "fuelName",
    labelKey: "propertyMention.fuel",
    format: (v) => v as string,
  },
  {
    key: "transmissionName",
    labelKey: "propertyMention.transmission",
    format: (v) => v as string,
  },
  {
    key: "municipalityName",
    labelKey: "propertyMention.city",
    format: (v) => v as string,
  },
];

const PropertyMentionPicker = ({
  listing,
  onSelect,
  onClose,
}: PropertyMentionPickerProps) => {
  const { t } = useTranslation("saved");

  return (
    <div className="border-border bg-card absolute z-10 mt-1 rounded border shadow-lg">
      <ul className="py-1">
        {PROPERTY_FIELDS.map(({ key, labelKey, format }) => {
          const label = t(labelKey);
          return (
            <li key={key}>
              <button
                className="hover:bg-muted w-full px-3 py-1.5 text-left text-sm"
                onClick={() => {
                  onSelect(`📌 ${label} · ${format(listing[key])}`);
                  onClose();
                }}
              >
                <span className="font-medium">{label}</span>
                <span className="text-muted-foreground ml-2">
                  {format(listing[key])}
                </span>
              </button>
            </li>
          );
        })}
      </ul>
    </div>
  );
};

export default PropertyMentionPicker;
