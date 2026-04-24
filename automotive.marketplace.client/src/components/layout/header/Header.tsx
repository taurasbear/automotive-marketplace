import { RegisterButton } from "@/features/auth";
import { UnreadBadge } from "@/features/chat";
import { useAppSelector } from "@/hooks/redux";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { IoHeart } from "react-icons/io5";
import LanguageSwitcher from "../../LanguageSwitcher";
import { Button } from "../../ui/button";
import ThemeToggle from "./ThemeToggle";
import UserMenu from "./UserMenu";

const Header = () => {
  const { t } = useTranslation("common");
  const { userId } = useAppSelector((state) => state.auth);

  return (
    <div className="dark:bg-secondary shadow-lg/2 dark:shadow-lg/4 dark:shadow-rose-600">
      <div className="mx-8 flex items-center justify-between py-2 xl:mx-auto xl:max-w-6xl">
        <div className="space-x-2 truncate">
          <Link to="/" className="[&.active]:font-bold">
            {t("header.title")}
          </Link>{" "}
        </div>
        <div className="flex items-center space-x-2 truncate">
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
          {userId ? <UserMenu /> : <RegisterButton />}
        </div>
      </div>
    </div>
  );
};

export default Header;
