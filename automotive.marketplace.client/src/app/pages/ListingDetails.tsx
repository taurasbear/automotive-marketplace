import { Route } from "@/app/routes/listing/$id";
import { ListingDetailsContent } from "@/features/listingDetails";

const ListingDetails = () => {
  const { id } = Route.useParams();

  return <ListingDetailsContent id={id} />;
};

export default ListingDetails;
