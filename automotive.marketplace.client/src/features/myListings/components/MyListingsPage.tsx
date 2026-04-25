import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { ChatPanel } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";

import { getMyListingsOptions } from "../api/getMyListingsOptions";
import MyListingCard from "./MyListingCard";

export default function MyListingsPage() {
  const { t } = useTranslation("myListings");
  const myListingsQuery = useQuery(getMyListingsOptions);

  const { data, isLoading, isError } = myListingsQuery;
  const listings = data?.data ?? [];

  const [activeChatConversation, setActiveChatConversation] =
    useState<ConversationSummary | null>(null);

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8 flex items-center justify-between">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-10 w-32" />
        </div>
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="rounded-lg border p-4">
              <div className="flex gap-4">
                <Skeleton className="h-24 w-32 shrink-0 rounded-lg" />
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
        <div className="text-center text-red-600">{t("page.loadError")}</div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Page Header */}
      <div className="mb-8 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">{t("page.title")}</h1>
        <Button asChild>
          <Link to="/listing/create">
            <Plus className="mr-2 h-4 w-4" />
            {t("page.createListing")}
          </Link>
        </Button>
      </div>

      {/* Listings */}
      {!listings || listings.length === 0 ? (
        <div className="py-12 text-center">
          <div className="mx-auto max-w-md">
            <h2 className="mb-2 text-xl font-medium text-gray-900">
              {t("page.emptyState")}
            </h2>
            <p className="mb-6 text-gray-600">{t("page.createFirst")}</p>
            <Button asChild>
              <Link to="/listing/create">
                <Plus className="mr-2 h-4 w-4" />
                {t("page.createListing")}
              </Link>
            </Button>
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          {listings.map((listing) => (
            <MyListingCard
              key={listing.id}
              listing={listing}
              onStartChat={setActiveChatConversation}
            />
          ))}
        </div>
      )}

      {/* Chat slide-over — same pattern as ListingDetailsContent */}
      {activeChatConversation && (
        <ChatPanel
          conversation={activeChatConversation}
          onClose={() => setActiveChatConversation(null)}
        />
      )}
    </div>
  );
}
