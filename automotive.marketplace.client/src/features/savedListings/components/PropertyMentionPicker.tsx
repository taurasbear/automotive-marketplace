import type { SavedListing } from "../types/SavedListing";

interface PropertyMentionPickerProps {
  listing: SavedListing;
  onSelect: (chip: string) => void;
  onClose: () => void;
}

const PROPERTY_FIELDS: {
  key: keyof SavedListing;
  label: string;
  format: (value: unknown) => string;
}[] = [
  {
    key: "mileage",
    label: "Mileage",
    format: (v) => `${(v as number).toLocaleString()} km`,
  },
  {
    key: "price",
    label: "Price",
    format: (v) => `${(v as number).toLocaleString()} €`,
  },
  { key: "fuelName", label: "Fuel", format: (v) => v as string },
  {
    key: "transmissionName",
    label: "Transmission",
    format: (v) => v as string,
  },
  { key: "city", label: "City", format: (v) => v as string },
];

const PropertyMentionPicker = ({
  listing,
  onSelect,
  onClose,
}: PropertyMentionPickerProps) => {
  return (
    <div className="border-border bg-card absolute z-10 mt-1 rounded border shadow-lg">
      <ul className="py-1">
        {PROPERTY_FIELDS.map(({ key, label, format }) => (
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
        ))}
      </ul>
    </div>
  );
};

export default PropertyMentionPicker;
