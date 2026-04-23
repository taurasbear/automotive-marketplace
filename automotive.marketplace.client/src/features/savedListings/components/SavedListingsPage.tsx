import { selectAccessToken } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import { useQuery } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { getSavedListingsOptions } from "../api/getSavedListingsOptions";
import SavedListingRow from "./SavedListingRow";

const SavedListingsPage = () => {
  const { t } = useTranslation("saved");
  const accessToken = useAppSelector(selectAccessToken);
  const { data: savedQuery } = useQuery({
    ...getSavedListingsOptions(),
    enabled: !!accessToken,
  });

  const listings = savedQuery?.data ?? [];

  if (listings.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <p className="text-muted-foreground text-lg">
          {t("page.emptyState")}
        </p>
        <Link
          to="/listings"
          className="mt-4 text-sm text-red-500 underline hover:text-red-600"
        >
          {t("page.browseListings")}
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl py-6">
      <h1 className="mb-4 text-2xl font-bold">{t("page.title")}</h1>
      <div className="border-border divide-border divide-y rounded border">
        {listings.map((listing) => (
          <SavedListingRow key={listing.listingId} listing={listing} />
        ))}
      </div>
    </div>
  );
};

export default SavedListingsPage;
