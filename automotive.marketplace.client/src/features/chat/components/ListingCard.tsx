import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

type ListingCardProps = {
  listingId: string;
  listingTitle: string;
  listingPrice: number;
  listingThumbnail: { url: string; altText: string } | null;
};

const ListingCard = ({
  listingId,
  listingTitle,
  listingPrice,
  listingThumbnail,
}: ListingCardProps) => {
  const { t } = useTranslation("chat");
  return (
    <div className="border-border bg-muted/40 flex items-center gap-3 border-b px-4 py-2">
      {listingThumbnail && (
        <img
          src={listingThumbnail.url}
          alt={listingThumbnail.altText || listingTitle}
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
        {t("listingCard.viewListing")}
      </Link>
    </div>
  );
};

export default ListingCard;
