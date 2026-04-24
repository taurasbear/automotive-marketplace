import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

import { getMyListingsOptions } from "../api/getMyListingsOptions";
import MyListingCard from "./MyListingCard";

export default function MyListingsPage() {
  const { t } = useTranslation("myListings");
  const myListingsQuery = useQuery(getMyListingsOptions);

  const { data, isLoading, isError } = myListingsQuery;

  const listings = data?.data ?? [];

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        {/* Header skeleton */}
        <div className="flex justify-between items-center mb-8">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-10 w-32" />
        </div>

        {/* Cards skeleton */}
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="border rounded-lg p-4">
              <div className="flex gap-4">
                <Skeleton className="w-32 h-24 rounded-lg shrink-0" />
                <div className="flex-1 space-y-3">
                  <Skeleton className="h-6 w-64" />
                  <Skeleton className="h-4 w-48" />
                  <div className="flex gap-2">
                    <Skeleton className="h-6 w-16" />
                    <Skeleton className="h-6 w-20" />
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center text-red-600">
          Failed to load listings. Please try again.
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Page Header */}
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          {t("page.title")}
        </h1>
        <Button asChild>
          <Link to="/listing/create">
            <Plus className="w-4 h-4 mr-2" />
            {t("page.createListing")}
          </Link>
        </Button>
      </div>

      {/* Listings */}
      {!listings || listings.length === 0 ? (
        /* Empty State */
        <div className="text-center py-12">
          <div className="max-w-md mx-auto">
            <h2 className="text-xl font-medium text-gray-900 mb-2">
              {t("page.emptyState")}
            </h2>
            <p className="text-gray-600 mb-6">
              {t("page.createFirst")}
            </p>
            <Button asChild>
              <Link to="/listing/create">
                <Plus className="w-4 h-4 mr-2" />
                {t("page.createListing")}
              </Link>
            </Button>
          </div>
        </div>
      ) : (
        /* Listings Grid */
        <div className="space-y-4">
          {listings.map((listing) => (
            <MyListingCard key={listing.id} listing={listing} />
          ))}
        </div>
      )}
    </div>
  );
}