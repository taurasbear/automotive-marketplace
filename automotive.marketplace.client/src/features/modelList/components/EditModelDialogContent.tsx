import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { getModelByIdOptions } from "../api/getModelByIdOptions";
import { ModelFormData } from "../types/ModelFormData";
import ModelForm from "./ModelForm";

type EditModelDialogContentProps = {
  id: string;
  onSubmit: (formData: ModelFormData) => Promise<void>;
};

const EditModelDialogContent = ({
  id,
  onSubmit,
}: EditModelDialogContentProps) => {
  const { t } = useTranslation("admin");
  const { data: modelQuery } = useSuspenseQuery(getModelByIdOptions({ id }));
  const model = modelQuery.data;
  return (
    <div>
      <DialogHeader>
        <DialogTitle>{t("models.editModel", { name: model.name })}</DialogTitle>
      </DialogHeader>
      <ModelForm
        model={{
          ...model,
        }}
        onSubmit={onSubmit}
      />
    </div>
  );
};

export default EditModelDialogContent;
