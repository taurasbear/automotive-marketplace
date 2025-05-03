import CarListings from "@/containers/CarListings";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listings")({
  component: CarListings,
});
