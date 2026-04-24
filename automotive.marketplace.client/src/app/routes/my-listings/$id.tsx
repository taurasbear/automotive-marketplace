import { createFileRoute } from "@tanstack/react-router";
import MyListingDetail from "@/features/myListings/components/MyListingDetail";

export const Route = createFileRoute("/my-listings/$id")({
  component: () => {
    const { id } = Route.useParams();
    return <MyListingDetail id={id} />;
  },
});
