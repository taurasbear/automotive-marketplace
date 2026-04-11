import { Link } from "@tanstack/react-router";

type ListingCardProps = {
  listingId: string;
  listingTitle: string;
  listingPrice: number;
  listingThumbnailUrl: string | null;
};

const ListingCard = ({
  listingId,
  listingTitle,
  listingPrice,
  listingThumbnailUrl,
}: ListingCardProps) => (
  <div className="border-border bg-muted/40 flex items-center gap-3 border-b px-4 py-2">
    {listingThumbnailUrl && (
      <img
        src={listingThumbnailUrl}
        alt={listingTitle}
        className="h-10 w-14 rounded object-cover"
      />
    )}
    <div className="min-w-0 flex-1">
      <p className="truncate text-sm font-semibold">{listingTitle}</p>
      <p className="text-muted-foreground text-xs">
        {listingPrice.toLocaleString()} €
      </p>
    </div>
    <Link
      to="/listing/$id"
      params={{ id: listingId }}
      className="text-primary shrink-0 text-xs hover:underline"
    >
      View listing
    </Link>
  </div>
);

export default ListingCard;
