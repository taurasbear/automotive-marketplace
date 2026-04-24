import { Button } from "@/components/ui/button";
import { useNavigate } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

const RegisterButton = () => {
  const { t } = useTranslation("common");
  const navigate = useNavigate();

  const handleRegister = async () => {
    await navigate({ to: "/register" });
  };

  return (
    <Button variant="secondary" onClick={() => void handleRegister()}>
      {t("header.signUp")}
    </Button>
  );
};

export default RegisterButton;
