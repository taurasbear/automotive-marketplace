import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

type CompareHeaderProps = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
};

const ListingCard = ({ listing }: { listing: GetListingByIdResponse }) => (
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
  </div>
);

export const CompareHeader = ({ listingA, listingB }: CompareHeaderProps) => (
  <div className="sticky top-0 z-10 mb-4 grid grid-cols-3 rounded-lg border bg-background p-4 shadow-sm">
    <div className="flex items-center">
      <span className="text-sm font-semibold text-muted-foreground">
        Specification
      </span>
    </div>
    <ListingCard listing={listingA} />
    <ListingCard listing={listingB} />
  </div>
);
