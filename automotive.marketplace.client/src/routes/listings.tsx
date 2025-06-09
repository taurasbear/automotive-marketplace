import CarListingsContainer from "@/containers/CarListingsContainer";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listings")({
  component: CarListingsContainer,
});
