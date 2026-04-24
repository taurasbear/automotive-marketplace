import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useCreateMake } from "../api/useCreateMake";
import { MakeFormData } from "../types/MakeFormData";
import MakeForm from "./MakeForm";

const CreateMakeDialog = () => {
  const [isOpen, setIsOpen] = useState(false);
  const { t } = useTranslation("admin");
  const { mutateAsync: createMakeAsync } = useCreateMake();

  const handleSubmit = async (formData: MakeFormData) => {
    await createMakeAsync({ ...formData });
    setIsOpen(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button>{t("makes.addMake")}</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("makes.createNewMake")}</DialogTitle>
        </DialogHeader>
        <MakeForm make={{ name: "" }} onSubmit={handleSubmit} />
      </DialogContent>
    </Dialog>
  );
};

export default CreateMakeDialog;
