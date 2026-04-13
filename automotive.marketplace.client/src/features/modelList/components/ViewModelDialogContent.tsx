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
        <p>Created by: {model.createdBy}</p>
        <p>Created at: {new Date(model.createdAt).toLocaleString()}</p>
        {model.modifiedAt && (
          <p>Last modified by: {model.modifiedBy} on {new Date(model.modifiedAt).toLocaleString()}</p>
        )}
      </div>
    </>
  );
};

export default ViewModelDialogContent;
