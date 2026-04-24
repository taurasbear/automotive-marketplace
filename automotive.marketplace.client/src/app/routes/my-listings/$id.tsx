import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/my-listings/$id")({
  component: () => <div>My Listing Detail Page (not implemented yet)</div>,
});