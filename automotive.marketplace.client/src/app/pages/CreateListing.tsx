import CreateListingForm from "@/features/createListing/components/CreateListingForm";

const CreateListing = () => {
  return (
    <div className="flex flex-col items-center py-24">
      <CreateListingForm className="w-full max-w-lg" />
    </div>
  );
};

export default CreateListing;
