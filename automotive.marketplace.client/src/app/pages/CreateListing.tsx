import { CreateListingForm } from "@/features/createListing";
import { selectAccessToken } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

const CreateListing = () => {
  const { t } = useTranslation("listings");
  const accessToken = useAppSelector(selectAccessToken);
  const isGuest = !accessToken;

  return (
    <div className="my-24 flex flex-col items-center space-y-12">
      <h1 className="text-xl font-semibold">{t("createPage.title")}</h1>
      {isGuest && (
        <div className="w-full rounded-lg border border-yellow-300 bg-yellow-50 px-4 py-3 text-sm text-yellow-800">
          {t("createPage.guestWarning.message")}{" "}
          <Link to="/login" className="font-semibold underline underline-offset-2">
            {t("createPage.guestWarning.signIn")}
          </Link>{" "}
          {t("createPage.guestWarning.or")}{" "}
          <Link to="/register" className="font-semibold underline underline-offset-2">
            {t("createPage.guestWarning.createAccount")}
          </Link>{" "}
          {t("createPage.guestWarning.toContinue")}
        </div>
      )}
      <CreateListingForm
        className="border-border bg-card w-full rounded-2xl border-1 p-4"
        isGuest={isGuest}
      />
    </div>
  );
};

export default CreateListing;
