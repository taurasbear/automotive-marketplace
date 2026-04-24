import { createFileRoute } from "@tanstack/react-router";
import MyListingsPage from "@/features/myListings/components/MyListingsPage";

export const Route = createFileRoute("/my-listings")({
  component: MyListingsPage,
});