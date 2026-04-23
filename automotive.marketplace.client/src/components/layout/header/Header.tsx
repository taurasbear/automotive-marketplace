import { PERMISSIONS } from "@/constants/permissions";
import { LogoutButton, RegisterButton } from "@/features/auth";
import { UnreadBadge } from "@/features/chat";
import { useAppSelector } from "@/hooks/redux";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { IoHeart } from "react-icons/io5";
import LanguageSwitcher from "../../LanguageSwitcher";
import { Button } from "../../ui/button";
import ThemeToggle from "./ThemeToggle";

const Header = () => {
  const { t } = useTranslation("common");
  const { userId, permissions } = useAppSelector((state) => state.auth);

  return (
    <div className="dark:bg-secondary shadow-lg/2 dark:shadow-lg/4 dark:shadow-rose-600">
      <div className="mx-8 flex items-center justify-between py-2 xl:mx-auto xl:max-w-6xl">
        <div className="space-x-2 truncate">
          <Link to="/" className="[&.active]:font-bold">
            {t("header.title")}
          </Link>{" "}
        </div>
        <div className="flex items-center space-x-2 truncate">
          {permissions.includes(PERMISSIONS.ViewMakes) && (
            <Link to="/makes">
              <Button variant="link">{t("header.makes")}</Button>
            </Link>
          )}
          {permissions.includes(PERMISSIONS.ViewModels) && (
            <Link to="/models">
              <Button variant="link">{t("header.models")}</Button>
            </Link>
          )}
          {permissions.includes(PERMISSIONS.ViewVariants) && (
            <Link to="/variants">
              <Button variant="link">{t("header.variants")}</Button>
            </Link>
          )}
          <Link to="/listing/create">
            <Button variant="link">{t("header.sellYourCar")}</Button>
          </Link>
          {userId && (
            <Link to="/inbox">
              <Button variant="link" className="relative">
                {t("header.inbox")}
                <UnreadBadge />
              </Button>
            </Link>
          )}
          {userId && (
            <Link to="/saved" title={t("header.savedListings")}>
              <Button variant="ghost" size="icon">
                <IoHeart className="h-5 w-5 text-red-500" />
              </Button>
            </Link>
          )}
          <LanguageSwitcher />
          <ThemeToggle />
          {userId ? <LogoutButton /> : <RegisterButton />}
        </div>
      </div>
    </div>
  );
};

export default Header;
