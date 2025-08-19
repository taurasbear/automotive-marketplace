import Listings from "@/pages/Listings";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listings")({
  component: Listings,
});
