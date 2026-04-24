import { createFileRoute } from "@tanstack/react-router";
import { MyListingsPage } from "@/features/myListings";

export const Route = createFileRoute("/my-listings/")({
  component: MyListingsPage,
});