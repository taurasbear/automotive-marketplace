import { Button } from "@/components/ui/button";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

type ListingCardProps = {
  listing: GetListingByIdResponse;
  onChange?: () => void;
  ariaLabel?: string;
};

const ListingCard = ({ listing, onChange, ariaLabel }: ListingCardProps) => (
  <div className="text-center">
    <img
      src={
        listing.images[0]?.url ??
        "https://placehold.co/200x150?text=No+Image"
      }
      alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
      className="mx-auto h-32 w-48 rounded object-cover"
    />
    <p className="mt-2 font-semibold">
      {listing.year} {listing.makeName} {listing.modelName}
    </p>
    <p className="text-primary font-bold">{listing.price.toFixed(0)} €</p>
    <p className="text-sm text-muted-foreground">{listing.city}</p>
    {onChange && (
      <Button
        variant="outline"
        size="sm"
        className="mt-2"
        aria-label={ariaLabel ?? "Change"}
        onClick={onChange}
      >
        Change
      </Button>
    )}
  </div>
);

type CompareHeaderProps = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
  onChangeA?: () => void;
  onChangeB?: () => void;
};

export const CompareHeader = ({
  listingA,
  listingB,
  onChangeA,
  onChangeB,
}: CompareHeaderProps) => (
  <div className="sticky top-0 z-10 mb-4 grid grid-cols-3 rounded-lg border bg-background p-4 shadow-sm">
    <div className="flex items-center">
      <span className="text-sm font-semibold text-muted-foreground">
        Specification
      </span>
    </div>
    <ListingCard listing={listingA} onChange={onChangeA} ariaLabel="Change listing A" />
    <ListingCard listing={listingB} onChange={onChangeB} ariaLabel="Change listing B" />
  </div>
);
