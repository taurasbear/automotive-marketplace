import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { useAppSelector } from "@/hooks/redux";
import { getSavedListingsOptions } from "@/features/savedListings/api/getSavedListingsOptions";
import type { SavedListing } from "@/features/savedListings/types/SavedListing";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { searchListingsOptions } from "../api/searchListingsOptions";

type CompareSearchModalProps = {
  open: boolean;
  onClose: () => void;
  excludeIds: string[];
  onSelect: (id: string) => void;
};

export const CompareSearchModal = ({
  open,
  onClose,
  excludeIds,
  onSelect,
}: CompareSearchModalProps) => {
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedQuery(query), 300);
    return () => clearTimeout(timer);
  }, [query]);

  useEffect(() => {
    if (!open) {
      setQuery("");
      setDebouncedQuery("");
    }
  }, [open]);

  const userId = useAppSelector((state) => state.auth.userId);

  const { data: savedData } = useQuery({
    ...getSavedListingsOptions(),
    enabled: userId !== null,
  });
  const savedListings: SavedListing[] = savedData?.data ?? [];

  const savedIdSet = useMemo(
    () => new Set(savedListings.map((s) => s.listingId)),
    [savedListings],
  );

  const { data: searchData } = useQuery(searchListingsOptions(debouncedQuery));
  const allResults = searchData?.data ?? [];

  const savedMatches = allResults.filter(
    (r) => savedIdSet.has(r.id) && !excludeIds.includes(r.id),
  );
  const otherResults = allResults.filter(
    (r) => !savedIdSet.has(r.id) && !excludeIds.includes(r.id),
  );

  const visibleSaved = savedListings.filter(
    (s) => !excludeIds.includes(s.listingId),
  );

  const isQueryEmpty = debouncedQuery === "";

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Compare with another listing</DialogTitle>
          <DialogDescription className="sr-only">
            Search for a listing to compare
          </DialogDescription>
        </DialogHeader>
        <Input
          placeholder="Search by make, model, year, seller…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          autoFocus
        />
        <div className="mt-4 max-h-96 space-y-2 overflow-y-auto">
          {isQueryEmpty && visibleSaved.length > 0 && (
            <>
              <p className="text-muted-foreground px-1 text-xs font-semibold uppercase tracking-wide">
                Your saved listings
              </p>
              {visibleSaved.map((listing) => (
                <div
                  key={listing.listingId}
                  className="flex items-center gap-3 rounded-lg border p-3"
                >
                  <img
                    src={
                      listing.thumbnail?.url ??
                      "https://placehold.co/80x60?text=No+Image"
                    }
                    alt={listing.title}
                    className="h-14 w-20 rounded object-cover"
                  />
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium">{listing.title}</p>
                    <p className="text-muted-foreground text-sm">
                      {listing.price.toFixed(0)} € ·{" "}
                      {listing.mileage.toLocaleString()} km · {listing.city}
                    </p>
                  </div>
                  <Button size="sm" onClick={() => onSelect(listing.listingId)}>
                    Compare
                  </Button>
                </div>
              ))}
            </>
          )}

          {!isQueryEmpty && (
            <>
              {[
                ...savedMatches.map((r) => ({ ...r, isSaved: true as const })),
                ...otherResults.map((r) => ({ ...r, isSaved: false as const })),
              ].map((listing) => (
                <div
                  key={listing.id}
                  className="flex items-center gap-3 rounded-lg border p-3"
                >
                  <img
                    src={
                      listing.firstImageUrl ??
                      "https://placehold.co/80x60?text=No+Image"
                    }
                    alt={`${listing.year} ${listing.makeName} ${listing.modelName}`}
                    className="h-14 w-20 rounded object-cover"
                  />
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium">
                      {listing.year} {listing.makeName} {listing.modelName}
                    </p>
                    <p className="text-muted-foreground text-sm">
                      {listing.price.toFixed(0)} € ·{" "}
                      {listing.mileage.toLocaleString()} km · {listing.city}
                    </p>
                    <p className="text-muted-foreground text-sm">
                      {listing.sellerName}
                    </p>
                  </div>
                  <div className="flex flex-col items-end gap-1">
                    {listing.isSaved && (
                      <span
                        aria-label="Saved listing"
                        className="rounded-full bg-red-100 px-2 py-0.5 text-xs font-semibold text-red-600"
                      >
                        ❤ Saved
                      </span>
                    )}
                    <Button size="sm" onClick={() => onSelect(listing.id)}>
                      Compare
                    </Button>
                  </div>
                </div>
              ))}
              {debouncedQuery &&
                savedMatches.length === 0 &&
                otherResults.length === 0 && (
                  <p className="text-muted-foreground py-4 text-center">
                    No results found
                  </p>
                )}
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
