import CreateListing from "@/app/pages/CreateListing";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listing/create")({
  component: CreateListing,
});
