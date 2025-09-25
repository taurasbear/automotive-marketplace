import ListingDetails from "@/app/pages/ListingDetails";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listing/$id")({
  component: ListingDetails,
});
