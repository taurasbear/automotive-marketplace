import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
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

  const { data } = useQuery(searchListingsOptions(debouncedQuery));
  const results = (data?.data ?? []).filter((r) => !excludeIds.includes(r.id));

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Compare with another listing</DialogTitle>
        </DialogHeader>
        <Input
          placeholder="Search by make, model, year, seller…"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          autoFocus
        />
        <div className="mt-4 max-h-96 space-y-2 overflow-y-auto">
          {results.map((listing) => (
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
              <Button size="sm" onClick={() => onSelect(listing.id)}>
                Compare
              </Button>
            </div>
          ))}
          {debouncedQuery && results.length === 0 && (
            <p className="text-muted-foreground py-4 text-center">
              No results found
            </p>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
