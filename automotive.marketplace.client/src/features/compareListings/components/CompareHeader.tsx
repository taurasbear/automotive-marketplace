import { Button } from "@/components/ui/button";
import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

type ListingCardProps = {
  listing: GetListingByIdResponse;
} & (
  | { onChange: () => void; ariaLabel: string }
  | { onChange?: undefined; ariaLabel?: undefined }
);

const ListingCard = ({ listing, onChange, ariaLabel }: ListingCardProps) => {
  const { t } = useTranslation("compare");

  return (
    <div className="text-center">
      <Link
        to="/listing/$id"
        params={{ id: listing.id }}
        className="group block text-center"
      >
        <img
          src={
            listing.images[0]?.url ?? "https://placehold.co/200x150?text=No+Image"
          }
          alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
          className="mx-auto h-32 w-48 rounded object-cover"
        />
        <p className="mt-2 font-semibold text-foreground group-hover:underline">
          {listing.year} {listing.makeName} {listing.modelName}
        </p>
      </Link>
      <p className="text-primary font-bold">{listing.price.toFixed(0)} €</p>
      <p className="text-muted-foreground text-sm">
        {listing.municipalityName}
      </p>
      {onChange && (
        <Button
          variant="outline"
          size="sm"
          className="mt-2"
          aria-label={ariaLabel}
          onClick={onChange}
        >
          {t("header.change")}
        </Button>
      )}
    </div>
  );
};

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
}: CompareHeaderProps) => {
  const { t } = useTranslation("compare");

  return (
    <div className="bg-background sticky top-0 z-10 mb-4 grid grid-cols-3 rounded-lg border p-4 shadow-sm">
      <div className="flex items-center">
        <span className="text-muted-foreground text-sm font-semibold">
          {t("header.specification")}
        </span>
      </div>
      {onChangeA ? (
        <ListingCard
          listing={listingA}
          onChange={onChangeA}
          ariaLabel={t("header.changeListingA")}
        />
      ) : (
        <ListingCard listing={listingA} />
      )}
      {onChangeB ? (
        <ListingCard
          listing={listingB}
          onChange={onChangeB}
          ariaLabel={t("header.changeListingB")}
        />
      ) : (
        <ListingCard listing={listingB} />
      )}
    </div>
  );
};
