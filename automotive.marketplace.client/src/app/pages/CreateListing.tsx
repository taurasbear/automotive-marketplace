import { CreateListingForm } from "@/features/createListing";

const CreateListing = () => {
  return (
    <div className="flex flex-col items-center py-24">
      <CreateListingForm className="w-full max-w-xl" />
    </div>
  );
};

export default CreateListing;
