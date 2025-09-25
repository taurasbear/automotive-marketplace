import { Button } from "@/components/ui/button";
import { PERMISSIONS } from "@/constants/permissions";
import { useAppSelector } from "@/hooks/redux";
import { router } from "@/lib/router";
import { useSuspenseQuery } from "@tanstack/react-query";
import { Trash } from "lucide-react";
import { getListingByIdOptions } from "../api/getListingByIdOptions";
import { useDeleteListing } from "../api/useDeleteListing";
import EditListingDialog from "./EditListingDialog";

type ListingDetailsProps = {
  id: string;
};

const ListingDetailsContent = ({ id }: ListingDetailsProps) => {
  const { data: listingQuery } = useSuspenseQuery(
    getListingByIdOptions({ id }),
  );

  const { mutateAsync: deleteListingAsync } = useDeleteListing();

  const listing = listingQuery.data;
  const { permissions } = useAppSelector((state) => state.auth);

  const canManageListing = permissions.includes(PERMISSIONS.ManageListings);

  const handleDelete = async () => {
    await deleteListingAsync({ id });
    await router.navigate({ to: "/" });
  };

  return (
    <div className="container mx-auto max-w-6xl p-4">
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-8 lg:col-span-2">
          <img
            className="aspect-video w-full rounded-lg object-cover shadow-lg"
            alt={
              listing.images[0]?.altText ||
              `${listing.year} ${listing.make} ${listing.model}`
            }
            src={
              listing.images.length > 0
                ? listing.images[0].url
                : "https://placehold.co/1280x720?text=Image+Not+Available"
            }
          />
          {listing.description && (
            <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
              <h2 className="mb-4 text-2xl font-semibold">Description</h2>
              <p className="text-muted-foreground">{listing.description}</p>
            </div>
          )}
        </div>

        <div className="space-y-6">
          <div className="bg-card text-card-foreground rounded-lg border p-6 shadow-sm">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h1 className="text-3xl font-bold">
                  {listing.year} {listing.make} {listing.model}
                </h1>
                <p className="text-primary mt-2 text-3xl font-semibold">
                  {listing.price.toFixed(2)} €
                </p>
              </div>
              {canManageListing && (
                <div className="flex flex-shrink-0 gap-2">
                  <EditListingDialog listing={listing} id={id} />
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={handleDelete}
                  >
                    <Trash />
                  </Button>
                </div>
              )}
            </div>
            <div className="mt-4 flex flex-wrap gap-2">
              <span className="bg-secondary text-secondary-foreground rounded-full border px-3 py-1 text-sm">
                {listing.isUsed ? "Used" : "New"}
              </span>
              <span className="bg-secondary text-secondary-foreground rounded-full border px-3 py-1 text-sm">
                {listing.city}
              </span>
            </div>
          </div>

          <div className="bg-card text-card-foreground rounded-lg border shadow-sm">
            <div className="p-6">
              <h2 className="text-2xl font-semibold">Specifications</h2>
            </div>
            <div className="border-t p-0">
              <dl className="divide-border divide-y">
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Mileage
                  </dt>
                  <dd className="text-right text-sm">
                    {listing.mileage.toLocaleString()} km
                  </dd>
                </div>
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Engine
                  </dt>
                  <dd className="text-right text-sm">
                    {listing.engineSize} ml, {listing.power} kw
                  </dd>
                </div>
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Transmission
                  </dt>
                  <dd className="text-right text-sm">{listing.transmission}</dd>
                </div>
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Drivetrain
                  </dt>
                  <dd className="text-right text-sm">{listing.drivetrain}</dd>
                </div>
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Fuel Type
                  </dt>
                  <dd className="text-right text-sm">{listing.fuel}</dd>
                </div>
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Body Type
                  </dt>
                  <dd className="text-right text-sm">{listing.bodyType}</dd>
                </div>
                {listing.colour && (
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      Colour
                    </dt>
                    <dd className="text-right text-sm">{listing.colour}</dd>
                  </div>
                )}
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Doors
                  </dt>
                  <dd className="text-right text-sm">{listing.doorCount}</dd>
                </div>
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Steering
                  </dt>
                  <dd className="text-right text-sm">
                    {listing.isSteeringWheelRight ? "Right-hand" : "Left-hand"}
                  </dd>
                </div>
                {listing.vin && (
                  <div className="grid grid-cols-2 px-6 py-3">
                    <dt className="text-muted-foreground text-sm font-medium">
                      VIN
                    </dt>
                    <dd className="text-right text-sm break-all">
                      {listing.vin}
                    </dd>
                  </div>
                )}
                <div className="grid grid-cols-2 px-6 py-3">
                  <dt className="text-muted-foreground text-sm font-medium">
                    Seller
                  </dt>
                  <dd className="text-right text-sm font-semibold">
                    {listing.seller}
                  </dd>
                </div>
              </dl>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ListingDetailsContent;
