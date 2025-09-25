import { Route } from "@/app/routes/listing/$id";
import ListingDetailsContent from "@/features/listingDetails/components/ListingDetailsContent";

const ListingDetails = () => {
  const { id } = Route.useParams();

  return <ListingDetailsContent id={id} />;
};

export default ListingDetails;
