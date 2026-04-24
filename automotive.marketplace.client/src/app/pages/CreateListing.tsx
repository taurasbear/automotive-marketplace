import { CreateListingForm } from "@/features/createListing";
import { useTranslation } from "react-i18next";

const CreateListing = () => {
  const { t } = useTranslation("listings");

  return (
    <div className="my-24 flex flex-col items-center space-y-12">
      <h1 className="text-xl font-semibold">{t("createPage.title")}</h1>
      <CreateListingForm className="border-border bg-card w-full rounded-2xl border-1 p-4" />
    </div>
  );
};

export default CreateListing;
