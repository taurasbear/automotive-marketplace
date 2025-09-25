import {
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getModelByIdOptions } from "../api/getModelByIdOptions";

type ViewModelDialogContentProps = {
  id: string;
  className?: string;
};

const ViewModelDialogContent = ({ id }: ViewModelDialogContentProps) => {
  const { data: modelQuery } = useSuspenseQuery(getModelByIdOptions({ id }));

  const model = modelQuery.data;

  return (
    <>
      <DialogHeader>
        <DialogTitle>Model details</DialogTitle>
        <DialogDescription>In depth</DialogDescription>
      </DialogHeader>
      <div className="grid gap-4">
        <h3>{model.name}</h3>
        <p>First appeared: {model.firstAppearanceDate}</p>
        {model.isDiscontinued ? (
          <p>Has been discontinued</p>
        ) : (
          <p>Has not been discontinued</p>
        )}
      </div>
    </>
  );
};

export default ViewModelDialogContent;
